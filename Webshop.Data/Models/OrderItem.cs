namespace Webshop.Data.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required int PriceAtPurchaseInOere { get; set; }
    }
}
