namespace RestaurantManagement.API.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        public int Quantity { get; set; }
    }
}
