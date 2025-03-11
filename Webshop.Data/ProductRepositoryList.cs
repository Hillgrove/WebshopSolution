using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public class ProductRepositoryList : IProductRepository
    {
        private int _nextId = 1;
        private readonly List<Product> _products = new();

        public ProductRepositoryList() { }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(new List<Product>(_products));
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return Task.FromResult(_products.FirstOrDefault(u => u.Id == id));
        }

        public Task<Product> AddAsync(Product newProduct)
        {
            newProduct.Id = _nextId++;
            _products.Add(newProduct);
            return Task.FromResult(newProduct);
        }

        public Task UpdateAsync(Product product)
        {
            var existingProduct = _products.FirstOrDefault(u => u.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
            }
            return Task.CompletedTask;
        }

        public Task<Product> Delete(Product x)
        {
            Product? product = _products.FirstOrDefault(u => u.Id == x.Id);
            if (product is null)
            {
                return null;
            }
            _products.Remove(product);
            //_products.SaveChanges();
            return Task.FromResult(product);
        }

    }
}
