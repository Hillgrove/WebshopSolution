using Webshop.Data.Models;

namespace Webshop.Data
{
    public class ProductRepositoryList : IProductRepository
    {
        private int _nextId = 1;
        private readonly List<Product> _products = new();

        public ProductRepositoryList()
        {
            _products.Add(new Product { Id = _nextId++, Name = "Product A", Description = "Product A description", Price = 111 });
            _products.Add(new Product { Id = _nextId++, Name = "Product B", Description = "Product B description", Price = 222 });
            _products.Add(new Product { Id = _nextId++, Name = "Product C", Description = "Product C description", Price = 333 });
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(new List<Product>(_products));
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
        }
    }
}
