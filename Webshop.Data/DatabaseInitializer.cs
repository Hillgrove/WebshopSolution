using System.Data.SQLite;

namespace Webshop.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        private readonly string CreateUsersTableSql = @"
        CREATE TABLE IF NOT EXISTS Users (
            ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            Email TEXT NOT NULL UNIQUE,
            Role TEXT NOT NULL DEFAULT 'Customer',
            PasswordHash TEXT NOT NULL,
            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PasswordResetToken TEXT NULL,
            PasswordResetTokenExpiration DATETIME NULL
        );";

        private readonly string CreateProductsTableSql = @"
        CREATE TABLE IF NOT EXISTS Products (
            ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            Name TEXT NOT NULL,
            Description TEXT NOT NULL,
            PriceInOere INTEGER NOT NULL DEFAULT 0 CHECK (PriceInOere >= 0)
        );";

        private readonly string CreateOrdersTableSql = @"
        CREATE TABLE IF NOT EXISTS Orders (
            ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            UserID INTEGER NOT NULL,
            TotalPriceInOere INTEGER NOT NULL CHECK (TotalPriceInOere >= 0),
            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE
        );";

        private readonly string CreateOrderItemsTableSql = @"
        CREATE TABLE IF NOT EXISTS OrderItems (
            ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            OrderID INTEGER NOT NULL,
            ProductID INTEGER,
            ProductName TEXT NOT NULL,
            Quantity INTEGER NOT NULL CHECK (Quantity > 0),
            PriceAtPurchaseInOere INTEGER NOT NULL DEFAULT 0 CHECK (PriceAtPurchaseInOere >= 0),
            FOREIGN KEY (OrderID) REFERENCES Orders(ID) ON DELETE CASCADE,
            FOREIGN KEY (ProductID) REFERENCES Products(ID) ON DELETE SET NULL
        );";

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await ExecuteTableCreation(connection, CreateUsersTableSql);
                await ExecuteTableCreation(connection, CreateProductsTableSql);
                await ExecuteTableCreation(connection, CreateOrdersTableSql);
                await ExecuteTableCreation(connection, CreateOrderItemsTableSql);

                await CreateGuestUserAsync(connection);
                await SeedProductsIfEmpty(connection);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task ExecuteTableCreation(SQLiteConnection connection, string sql)
        {
            using var command = new SQLiteCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task CreateGuestUserAsync(SQLiteConnection connection)
        {
            var insertCommand = new SQLiteCommand(@"
                INSERT OR IGNORE INTO Users(ID, Email, Role, PasswordHash, CreatedAt)
                VALUES(-1, 'guest', 'Guest', '', CURRENT_TIMESTAMP);", connection);
            await insertCommand.ExecuteNonQueryAsync();
        }

        private static async Task SeedProductsIfEmpty(SQLiteConnection connection)
        {
            using var countCommand = new SQLiteCommand("SELECT COUNT(*) FROM Products", connection);
            var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                using var insertCommand = new SQLiteCommand(@"
                INSERT INTO Products (Name, Description, PriceInOere) VALUES
                ('Product A', 'Description of product A', 1999),
                ('Product B', 'Description of product B', 2999),
                ('Product C', 'Description of product C', 1499);", connection);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
