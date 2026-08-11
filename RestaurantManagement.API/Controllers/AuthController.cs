using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // ============================================================
        // LOGIN
        // POST: api/auth/login
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Username and password are required."
                });
            }

            var user = await _userManager
                .FindByNameAsync(request.UserName);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid credentials."
                });
            }

            var passwordValid = await _userManager
                .CheckPasswordAsync(user, request.Password);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid credentials."
                });
            }

            var roles = await _userManager
                .GetRolesAsync(user);

            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                token,

                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    RestaurantId = user.RestaurantId,
                    Roles = roles
                }
            });
        }

        // ============================================================
        // REGISTER NORMAL USER
        // POST: api/auth/register
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Invalid registration request."
                });
            }

            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(new
                {
                    message = "Username is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Password is required."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            // Check username
            var existingUser = await _userManager
                .FindByNameAsync(request.UserName.Trim());

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "Username is already taken."
                });
            }

            // Check email
            var existingEmail = await _userManager
                .FindByEmailAsync(request.Email.Trim());

            if (existingEmail != null)
            {
                return BadRequest(new
                {
                    message = "Email is already registered."
                });
            }

            // Create a normal user.
            // RestaurantId remains null because this is NOT
            // a RestaurantAdmin.
            var user = new ApplicationUser
            {
                UserName = request.UserName.Trim(),
                Email = request.Email.Trim(),
                RestaurantId = null
            };

            var result = await _userManager
                .CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description)
                    .ToList();

                return BadRequest(new
                {
                    message = "Registration failed.",
                    errors
                });
            }

            // IMPORTANT:
            // Every person registering through the website
            // gets ONLY the "User" role.
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new
            {
                message = "Registration successful. You can now log in.",
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    RestaurantId = user.RestaurantId,
                    Role = "User"
                }
            });
        }

        // ============================================================
        // GENERATE JWT
        // ============================================================
        private string GenerateJwtToken(
            ApplicationUser user,
            IList<string> roles)
        {
            var jwtSettings =
                _configuration.GetSection("JwtSettings");

            var secretKey =
                jwtSettings.GetValue<string>("Secret")
                ?? "RestaurantManagementSecretKey12345";

            var issuer =
                jwtSettings.GetValue<string>("Issuer")
                ?? "RestaurantManagementAPI";

            var audience =
                jwtSettings.GetValue<string>("Audience")
                ?? "RestaurantManagementClient";

            var expiryMinutes =
                jwtSettings.GetValue<int>("ExpiryMinutes");

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id
                ),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()
                ),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName ?? string.Empty
                )
            };

            // Add roles to JWT
            claims.AddRange(
                roles.Select(
                    role => new Claim(
                        ClaimTypes.Role,
                        role
                    )
                )
            );

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)
                );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    expiryMinutes > 0
                        ? expiryMinutes
                        : 1440
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }

    // ================================================================
    // LOGIN REQUEST
    // ================================================================
    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    // ================================================================
    // REGISTER REQUEST
    // ================================================================
    public class RegisterRequest
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
