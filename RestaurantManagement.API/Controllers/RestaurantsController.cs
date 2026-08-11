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
    [Authorize]
    public class RestaurantsController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public RestaurantsController(RestaurantContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: api/restaurants
        // Anyone can view active restaurants
        // ============================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRestaurants()
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.IsActive)
                .ToListAsync();

            return Ok(restaurants);
        }

        // ============================================================
        // GET: api/restaurants/all
        // ONLY SuperAdmin can view all restaurants
        // ============================================================
        [HttpGet("all")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllRestaurants()
        {
            var restaurants = await _context.Restaurants
                .ToListAsync();

            return Ok(restaurants);
        }

        // ============================================================
        // GET: api/restaurants/{id}
        // SuperAdmin -> any restaurant
        // RestaurantAdmin -> own restaurant only
        // Normal User -> can view restaurant
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound(new
                {
                    message = "Restaurant not found."
                });
            }

            // SuperAdmin can access any restaurant
            if (User.IsInRole("SuperAdmin"))
            {
                return Ok(restaurant);
            }

            // RestaurantAdmin can access only their own restaurant
            if (User.IsInRole("RestaurantAdmin"))
            {
                if (!await CanAccessRestaurant(id))
                {
                    return Forbid();
                }

                return Ok(restaurant);
            }

            // Normal users can view active restaurants
            if (User.IsInRole("User"))
            {
                if (!restaurant.IsActive)
                {
                    return NotFound(new
                    {
                        message = "Restaurant is not active."
                    });
                }

                return Ok(restaurant);
            }

            return Forbid();
        }

        // ============================================================
        // POST: api/restaurants
        // ONLY SuperAdmin can create a restaurant
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateRestaurant(
            [FromBody] Restaurant restaurant)
        {
            if (restaurant == null)
            {
                return BadRequest(new
                {
                    message = "Invalid restaurant data."
                });
            }

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRestaurant),
                new { id = restaurant.Id },
                restaurant
            );
        }

        // ============================================================
        // PUT: api/restaurants/{id}
        // ONLY SuperAdmin can fully update a restaurant
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateRestaurant(
            int id,
            [FromBody] Restaurant restaurant)
        {
            if (restaurant == null)
            {
                return BadRequest(new
                {
                    message = "Invalid restaurant data."
                });
            }

            if (id != restaurant.Id)
            {
                return BadRequest(new
                {
                    message = "Restaurant ID does not match."
                });
            }

            var existing = await _context.Restaurants
                .FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = restaurant.Name;
            existing.Address = restaurant.Address;
            existing.IsActive = restaurant.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        // PUT: api/restaurants/{id}/profile
        // RestaurantAdmin -> own restaurant
        // SuperAdmin -> any restaurant
        // ============================================================
        [HttpPut("{id}/profile")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> UpdateRestaurantProfile(
            int id,
            [FromBody] Restaurant restaurant)
        {
            if (restaurant == null)
            {
                return BadRequest(new
                {
                    message = "Invalid restaurant data."
                });
            }

            if (id != restaurant.Id)
            {
                return BadRequest(new
                {
                    message = "Restaurant ID does not match."
                });
            }

            var existing = await _context.Restaurants
                .FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            // IMPORTANT:
            // RestaurantAdmin can only update their own restaurant.
            // SuperAdmin can update any restaurant.
            if (!await CanAccessRestaurant(id))
            {
                return Forbid();
            }

            existing.Name = restaurant.Name;
            existing.Address = restaurant.Address;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        // GET: api/restaurants/dashboard
        // RestaurantAdmin -> own restaurant
        // SuperAdmin -> any restaurant
        // ============================================================
        [HttpGet("dashboard")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> GetRestaurantDashboard(
            [FromQuery] int? restaurantId = null)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FindAsync(userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var targetRestaurantId = restaurantId;

            // RestaurantAdmin is ALWAYS restricted to own restaurant
            if (User.IsInRole("RestaurantAdmin"))
            {
                targetRestaurantId = user.RestaurantId;
            }

            // SuperAdmin must provide a restaurant ID
            if (!targetRestaurantId.HasValue)
            {
                return BadRequest(new
                {
                    message = "Restaurant ID is required."
                });
            }

            if (!await CanAccessRestaurant(
                    targetRestaurantId.Value))
            {
                return Forbid();
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.MenuItems)
                .Include(r => r.Orders)
                .FirstOrDefaultAsync(
                    r => r.Id == targetRestaurantId.Value);

            if (restaurant == null)
            {
                return NotFound();
            }

            var pendingOrders = restaurant.Orders
                .Count(o => o.Status == OrderStatus.Pending);

            var completedOrders = restaurant.Orders
                .Count(o => o.Status == OrderStatus.Completed);

            var totalSales = restaurant.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .Sum(o => o.TotalPrice);

            return Ok(new
            {
                restaurant.Id,
                restaurant.Name,
                restaurant.Address,
                restaurant.IsActive,
                MenuItemCount = restaurant.MenuItems.Count,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                TotalSales = totalSales
            });
        }

        // ============================================================
        // CHECK RESTAURANT ACCESS
        // ============================================================
        private async Task<bool> CanAccessRestaurant(
            int restaurantId)
        {
            // SuperAdmin has access to everything
            if (User.IsInRole("SuperAdmin"))
            {
                return true;
            }

            // Only RestaurantAdmin should reach this logic
            if (!User.IsInRole("RestaurantAdmin"))
            {
                return false;
            }

            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = await _context.Users
                .FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            return user.RestaurantId == restaurantId;
        }

        // ============================================================
        // DELETE: api/restaurants/{id}
        // ONLY SuperAdmin
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .FindAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            // Soft delete
            restaurant.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}