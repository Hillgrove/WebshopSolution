
namespace Webshop.Data.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required int PriceInOere { get; set; }
    }
}
