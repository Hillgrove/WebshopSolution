namespace Webshop.Data.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required List<OrderItem> Items { get; set; }
        public required int TotalPriceInOere { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
