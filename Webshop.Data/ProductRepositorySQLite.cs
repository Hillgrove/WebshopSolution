using System.Data.SQLite;
using Webshop.Data.Models;

namespace Webshop.Data
{
    // TODO: Check if I need all the properties in the various commands
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

        public async Task<Product> AddAsync(Product newProduct)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // Insert new user
            var insertCommand = new SQLiteCommand(connection)
            {
                CommandText = @"
                    INSERT INTO Products (Name, Description, PriceInOere)
                    VALUES (@Name, @Description, @PriceInOere);
                    SELECT last_insert_rowid()"
            };

            insertCommand.Parameters.AddWithValue("@Name", newProduct.Name);
            insertCommand.Parameters.AddWithValue("@Description", newProduct.Description);
            insertCommand.Parameters.AddWithValue("@PriceInOere", newProduct.PriceInOere);

            var result = await insertCommand.ExecuteScalarAsync();
            if (result == null)
            {
                throw new Exception("Database operation failed: No ID was returned.");
            }

            newProduct.Id = Convert.ToInt32(result);
            return newProduct;
        }

        public async Task UpdateAsync(Product product)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                    UPDATE Products
                    SET Name = @Name, Description = @Description, PriceInOere = @PriceInOere
                    WHERE Id = @Id"
            };

            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", product.Description);
            command.Parameters.AddWithValue("@PriceInOere", product.PriceInOere);
            command.Parameters.AddWithValue("@Id", product.Id);

            await command.ExecuteNonQueryAsync();
        }

        public Task<Product> Delete(Product x)
        {
            throw new NotImplementedException();
        }
    }
}
