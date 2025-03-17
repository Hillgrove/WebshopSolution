using System.Data.SQLite;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public class OrderRepositorySQLite
    {
        private readonly string _connectionString;

        public OrderRepositorySQLite(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SaveAsync(Order order)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var insertOrderCommand = new SQLiteCommand(connection)
                {
                    CommandText = "INSERT INTO Orders (userID, TotalPriceInOere, CreatedAt) " +
                                  "VALUES (@UserId, @TotalPriceInOere, @CreatedAt); " +
                                  "SELECT last_insert_rowid();"
                };

                insertOrderCommand.Parameters.AddWithValue("@UserId", order.UserId);
                insertOrderCommand.Parameters.AddWithValue("@TotalPriceInOere", order.TotalPriceInOere);
                insertOrderCommand.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

                var orderId = Convert.ToInt32(await insertOrderCommand.ExecuteScalarAsync());

                foreach (var item in order.Items)
                {
                    var insertItemCommand = new SQLiteCommand(connection)
                    {
                        CommandText = "INSERT INTO OrderItems (OrderID, ProductID, ProductName, Quantity, PriceAtPurchaseInOere) " +
                                      "VALUES (@OrderID, @ProductID, @ProductName, @Quantity, @PriceAtPurchaseInOere)"
                    };

                    insertItemCommand.Parameters.AddWithValue("@OrderID", orderId);
                    insertItemCommand.Parameters.AddWithValue("@ProductID", item.ProductId);
                    insertItemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                    insertItemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                    insertItemCommand.Parameters.AddWithValue("@PriceAtPurchaseInOere", item.PriceInOere);

                    await insertItemCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
