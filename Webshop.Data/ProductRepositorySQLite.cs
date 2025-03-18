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

        public async Task InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var createTableCommand = new SQLiteCommand(connection)
            {
                CommandText = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        PriceInOere INTEGER NOT NULL DEFAULT 0,
                        CreatedAt DATETIME NOT NULL
                    )"
            };

            await createTableCommand.ExecuteNonQueryAsync();

            // Check if table is empty
            var countCommand = new SQLiteCommand("SELECT COUNT(*) FROM Products", connection);
            var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            if (count == 0) // Only insert if table is empty
            {
                var insertCommand = new SQLiteCommand(connection)
                {
                    CommandText = @"
                INSERT INTO Products (Name, Description, PriceInOere) VALUES
                ('Produkt A', 'Beskrivelse af produkt A', 1999),
                ('Produkt B', 'Beskrivelse af produkt B', 2999),
                ('Produkt C', 'Beskrivelse af produkt C', 1499);"
                };

                await insertCommand.ExecuteNonQueryAsync();
            }
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
                    Price = reader.GetInt32(3) / 100m  // Convert from øre to kroner
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
                    Price = reader.GetDecimal(3) / 100m  // Convert from øre to kroner
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
            insertCommand.Parameters.AddWithValue("@Price", newProduct.Price);
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
            command.Parameters.AddWithValue("@PasswordHash", product.Price);
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
