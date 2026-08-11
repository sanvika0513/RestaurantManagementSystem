using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
