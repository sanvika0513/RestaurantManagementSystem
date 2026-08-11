using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public MenuController(RestaurantContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<MenuItem>> GetMenuItems()
        {
            return await _context.MenuItems.Where(m => m.IsAvailable).ToListAsync();
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetMenuForRestaurant(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null || !restaurant.IsActive)
            {
                return NotFound();
            }

            var menu = await _context.MenuItems.Where(m => m.RestaurantId == restaurantId && m.IsAvailable).ToListAsync();
            return Ok(menu);
        }

        [HttpGet("restaurant/{restaurantId}/all")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> GetMenuForRestaurantAdmin(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return NotFound();
            }

            if (!await CanAccessRestaurant(restaurantId))
            {
                return Forbid();
            }

            var menu = await _context.MenuItems.Where(m => m.RestaurantId == restaurantId).ToListAsync();
            return Ok(menu);
        }

            [HttpPost]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<ActionResult<MenuItem>> CreateMenuItem(MenuItem menuItem)
        {
            if (!await CanAccessRestaurant(menuItem.RestaurantId))
            {
                return Forbid();
            }

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMenuForRestaurant), new { restaurantId = menuItem.RestaurantId }, menuItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> UpdateMenuItem(int id, MenuItem menuItem)
        {
            if (id != menuItem.Id)
            {
                return BadRequest();
            }

            var existing = await _context.MenuItems.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!await CanAccessRestaurant(existing.RestaurantId))
            {
                return Forbid();
            }

            existing.Name = menuItem.Name;
            existing.Description = menuItem.Description;
            existing.Price = menuItem.Price;
            existing.IsAvailable = menuItem.IsAvailable;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var existing = await _context.MenuItems.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!await CanAccessRestaurant(existing.RestaurantId))
            {
                return Forbid();
            }

            existing.IsAvailable = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> CanAccessRestaurant(int restaurantId)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return true;
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            return user != null && user.RestaurantId == restaurantId;
        }
    }
}
