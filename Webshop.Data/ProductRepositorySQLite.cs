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
                        PriceInOere INTEGER NOT NULL DEFAULT 0
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
                };
            }

            return null;
        }
    }
}
