using Webshop.Data.Models;

namespace Webshop.Data
{
    public class ProductRepositoryList : IProductRepository
    {
        private int _nextId = 1;
        private readonly List<Product> _products = new();

        public ProductRepositoryList()
        {
            AddAsync(new Product { Id = _nextId++, Name = "Product Name", Description = "Product Description", Price = 2, CreatedAt = new DateTime(2025, 3, 17, 14, 30, 0)});
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(new List<Product>(_products));
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
        }

        public Task<Product> AddAsync(Product newProduct)
        {
            newProduct.Id = _nextId++;
            _products.Add(newProduct);
            return Task.FromResult(newProduct);
        }

        public Task UpdateAsync(Product product)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
            }
            return Task.CompletedTask;
        }

        public Task<Product?> Delete(Product productToDelete)
        {
            Product? deletedProduct = _products.FirstOrDefault(p => p.Id == productToDelete.Id);
            if (deletedProduct is null)
            {
                return Task.FromResult<Product?>(null);
            }
            _products.Remove(deletedProduct);
            return Task.FromResult<Product?>(deletedProduct);
        }
    }
}
