using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<ApplicationUser> Admins { get; set; } = new List<ApplicationUser>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
