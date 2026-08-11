using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public CartController(RestaurantContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var items = await _context.CartItems
                .Include(c => c.MenuItem)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddCartItemRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var menuItem = await _context.MenuItems.FindAsync(request.MenuItemId);
            if (menuItem == null || !menuItem.IsAvailable)
            {
                return BadRequest(new { message = "Menu item not available." });
            }

            var existing = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.MenuItemId == request.MenuItemId);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    MenuItemId = request.MenuItemId,
                    Quantity = request.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, UpdateCartItemRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existing = await _context.CartItems.FindAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                return NotFound();
            }

            existing.Quantity = request.Quantity;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existing = await _context.CartItems.FindAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                return NotFound();
            }

            _context.CartItems.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class AddCartItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}
