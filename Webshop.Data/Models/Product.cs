namespace Webshop.Data.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int PriceInOere { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
