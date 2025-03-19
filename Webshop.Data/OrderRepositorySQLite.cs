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

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var orders = new List<Order>();

            var getOrdersCommand = new SQLiteCommand(connection)
            {
                CommandText = "SELECT ID, TotalPriceInOere, CreatedAt " +
                              "FROM Orders WHERE UserID = @UserId " +
                              "ORDER BY CreatedAt DESC"
            };

            getOrdersCommand.Parameters.AddWithValue("@UserId", userId);

            using var orderReader = await getOrdersCommand.ExecuteReaderAsync();
            while (await orderReader.ReadAsync())
            {
                var order = new Order
                {
                    Id = orderReader.GetInt32(0),
                    UserId = userId,
                    TotalPriceInOere = orderReader.GetInt32(1),
                    CreatedAt = orderReader.GetDateTime(2),
                    Items = new List<OrderItem>()
                };

                orders.Add(order);
            }

            foreach (var order in orders)
            {
                var getItemCommand = new SQLiteCommand(connection)
                {
                    CommandText = "SELECT ProductID, ProductName, Quantity, PriceAtPurchaseInOere " +
                                  "FROM OrderItems " +
                                  "WHERE OrderID = @OrderId"
                };

                getItemCommand.Parameters.AddWithValue("@OrderId", order.Id);

                using var itemReader = await getItemCommand.ExecuteReaderAsync();
                while (await itemReader.ReadAsync())
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = itemReader.GetInt32(0),
                        ProductName = itemReader.GetString(1),
                        Quantity = itemReader.GetInt32(2),
                        PriceAtPurchaseInOere = itemReader.GetInt32(3),
                    });
                }
            }

            return orders;
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

                await insertOrderCommand.ExecuteNonQueryAsync();
                var orderId = connection.LastInsertRowId;
                if (orderId == 0) throw new Exception("Failed to insert order.");

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
                    insertItemCommand.Parameters.AddWithValue("@PriceAtPurchaseInOere", item.PriceAtPurchaseInOere);

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
