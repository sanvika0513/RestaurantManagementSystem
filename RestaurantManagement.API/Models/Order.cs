using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagement.API.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        InPreparation,
        Ready,
        Completed,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalPrice { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
