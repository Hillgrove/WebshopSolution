namespace Webshop.Data.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public required int ProductId { get; set; }
        public string? ProductName { get; set; }
        public required int Quantity { get; set; }
        public required int PriceInOere { get; set; }
    }
}
