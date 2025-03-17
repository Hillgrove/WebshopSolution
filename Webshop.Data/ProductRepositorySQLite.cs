using System.Data.SQLite;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public class ProductRepositorySQLite : IProductRepository
    {
        private readonly string _connectionString;

        
        public ProductRepositorySQLite(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = new List<Product>();

            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Products", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var product = new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    PriceInOere = reader.GetInt32(3)
                };

                products.Add(product);
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Products WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    PriceInOere = reader.GetInt32(3)
                };
            }

            return null;
        }        
    }
}
