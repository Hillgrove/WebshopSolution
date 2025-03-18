using Webshop.Data.Models;

namespace Webshop.Data
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> AddAsync(Product newProduct);
        Task UpdateAsync(Product product);
        Task<Product> Delete(Product x);

    }
}