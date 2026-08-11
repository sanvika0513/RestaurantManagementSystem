using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(RestaurantContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var summaries = new List<UserSummary>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                summaries.Add(new UserSummary
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    RestaurantId = user.RestaurantId,
                    Roles = roles.ToList()
                });
            }

            return Ok(summaries);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                return BadRequest(new { message = "Role does not exist." });
            }

            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                RestaurantId = request.RestaurantId
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, request.Role);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.UserName, user.Email, request.Role, user.RestaurantId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = request.Email ?? user.Email;
            user.RestaurantId = request.RestaurantId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    return BadRequest(new { message = "Role does not exist." });
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, request.Role);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }

    public class UserSummary
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? RestaurantId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int? RestaurantId { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public int? RestaurantId { get; set; }
    }
}
