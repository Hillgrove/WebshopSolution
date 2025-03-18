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
                    PriceInOere = reader.GetInt32(3),
                    // TODO: set in db or backend?
                    CreatedAt = reader.GetDateTime(4),
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
                    PriceInOere = reader.GetInt32(3),
                    CreatedAt = reader.GetDateTime(4),
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
                    INSERT INTO Products (Name, Description, Price, CreatedAt)
                    VALUES (@Name, @Description, @Price, @CreatedAt);
                    SELECT last_insert_rowid()"
            };

            insertCommand.Parameters.AddWithValue("@Name", newProduct.Name);
            insertCommand.Parameters.AddWithValue("@Description", newProduct.Description);
            insertCommand.Parameters.AddWithValue("@Price", newProduct.PriceInOere);
            insertCommand.Parameters.AddWithValue("@CreatedAt", newProduct.CreatedAt);

            newProduct.Id = Convert.ToInt32(insertCommand.ExecuteScalar());
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
                    SET Name = @Name, Description = @Description, Price = @Price, CreatedAt = @CreatedAt,
                    WHERE Id = @Id"
            };

            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@PasswordHash", product.Description);
            command.Parameters.AddWithValue("@PasswordHash", product.PriceInOere);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            command.Parameters.AddWithValue("@Id", product.Id);

            await command.ExecuteNonQueryAsync();
        }

        public Task<Product> Delete(Product x)
        {
            throw new NotImplementedException();
        }
    }
}
