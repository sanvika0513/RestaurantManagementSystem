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
    public class OrdersController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public OrdersController(RestaurantContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: api/orders/my
        // Normal user - view their own orders
        // ============================================================
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        // ============================================================
        // POST: api/orders
        // Normal user - place an order
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder(
            [FromBody] PlaceOrderRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            if (request == null)
            {
                return BadRequest(new { message = "Invalid order request." });
            }

            var restaurant = await _context.Restaurants
                .FindAsync(request.RestaurantId);

            if (restaurant == null || !restaurant.IsActive)
            {
                return BadRequest(new
                {
                    message = "Restaurant not found or inactive."
                });
            }

            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(new
                {
                    message = "Order must contain at least one item."
                });
            }

            if (request.Items.Any(i => i.Quantity <= 0))
            {
                return BadRequest(new
                {
                    message = "Quantity must be greater than zero."
                });
            }

            var requestedMenuItemIds =
                request.Items.Select(i => i.MenuItemId).Distinct().ToList();

            var menuItems = await _context.MenuItems
                .Where(m => requestedMenuItemIds.Contains(m.Id))
                .ToListAsync();

            if (menuItems.Count != requestedMenuItemIds.Count)
            {
                return BadRequest(new
                {
                    message = "One or more menu items are invalid."
                });
            }

            if (menuItems.Any(m => m.RestaurantId != request.RestaurantId))
            {
                return BadRequest(new
                {
                    message = "All items must belong to the selected restaurant."
                });
            }

            var order = new Order
            {
                UserId = userId,
                RestaurantId = request.RestaurantId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0m
            };

            foreach (var line in request.Items)
            {
                var menuItem = menuItems
                    .First(m => m.Id == line.MenuItemId);

                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = line.Quantity,
                    UnitPrice = menuItem.Price
                };

                order.OrderItems.Add(orderItem);

                order.TotalPrice +=
                    menuItem.Price * line.Quantity;
            }

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // Clear the user's cart after successful order
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }

            return Ok(order);
        }

        // ============================================================
        // GET: api/orders/restaurant/{restaurantId}
        // RestaurantAdmin/SuperAdmin - view restaurant orders
        // ============================================================
        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> GetRestaurantOrders(int restaurantId)
        {
            if (!await CanAccessRestaurant(restaurantId))
            {
                return Forbid();
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        // ============================================================
        // PUT: api/orders/{orderId}/status
        // RestaurantAdmin/SuperAdmin - update order status
        // ============================================================
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "RestaurantAdmin,SuperAdmin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            [FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Invalid status request."
                });
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(new
                {
                    message = $"Order {orderId} was not found."
                });
            }

            // IMPORTANT:
            // RestaurantAdmin can update ONLY orders belonging
            // to their own restaurant.
            if (!await CanAccessRestaurant(order.RestaurantId))
            {
                return Forbid();
            }

            order.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order status updated successfully.",
                orderId = order.Id,
                status = order.Status
            });
        }

        // ============================================================
        // Check whether current user can access a restaurant
        // ============================================================
        private async Task<bool> CanAccessRestaurant(int restaurantId)
        {
            // SuperAdmin can access every restaurant
            if (User.IsInRole("SuperAdmin"))
            {
                return true;
            }

            // RestaurantAdmin must belong to the restaurant
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
    }

    // ================================================================
    // Request models
    // ================================================================

    public class PlaceOrderRequest
    {
        public int RestaurantId { get; set; }

        public List<OrderItemRequest> Items { get; set; }
            = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public int MenuItemId { get; set; }

        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}