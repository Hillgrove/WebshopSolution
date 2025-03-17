
namespace Webshop.Data.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public int PriceInOere { get; set; }
    }
}
