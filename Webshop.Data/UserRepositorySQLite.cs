using System.Data.SQLite;
using Webshop.Data.Models;

namespace Webshop.Data
{
    // TODO: Check if I need all the properties in the various commands
    public class UserRepositorySQLite : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepositorySQLite(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // TODO: If more tables/entities are added, move this to a separate DatabaseInitializer class.
            var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    PasswordResetToken TEXT,
                    PasswordResetTokenExpiration DATETIME
                )"
            };

            command.ExecuteNonQuery();
        }

        public async Task<User> AddAsync(User newUser)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // Insert new user
            var insertCommand = new SQLiteCommand(connection)
            {
                CommandText = @"
                    INSERT INTO Users (Email, PasswordHash, CreatedAt)
                    VALUES (@Email, @PasswordHash, @CreatedAt);
                    SELECT last_insert_rowid()"
            };

            insertCommand.Parameters.AddWithValue("@Email", newUser.Email);
            insertCommand.Parameters.AddWithValue("@PasswordHash", newUser.PasswordHash);
            insertCommand.Parameters.AddWithValue("CreatedAt", newUser.CreatedAt);

            newUser.Id = Convert.ToInt32(insertCommand.ExecuteScalar());
            return newUser;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = new List<User>();

            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Users", connection);
            using var reader = command.ExecuteReader();

            while (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    PasswordResetToken = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PasswordResetTokenExpiration = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                };

                users.Add(user);
            }

            return users;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Users WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    PasswordResetToken = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PasswordResetTokenExpiration = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                };
            }

            return null;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Users WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    PasswordResetToken = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PasswordResetTokenExpiration = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                };
            }

            return null;
        }

        public async Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiration)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                    UPDATE Users
                    SET PasswordResetToken = @Token, PasswordResetTokenExpiration = @Expiration
                    WHERE Id = @UserId"
            };

            command.Parameters.AddWithValue("@Token", token);
            command.Parameters.AddWithValue("@Expiration", expiration);
            command.Parameters.AddWithValue("@UserId", userId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<User?> GetUserByPasswordResetTokenAsync(string token)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Users WHERE PasswordResetToken = @Token", connection);
            command.Parameters.AddWithValue("@Token", token);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    PasswordResetToken = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PasswordResetTokenExpiration = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                };
            }

            return null;
        }

        public async Task UpdateAsync(User user)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                    UPDATE Users
                    SET Email = @Email, PasswordHash = @PasswordHash, CreatedAt = @CreatedAt,
                        PasswordResetToken = @PasswordResetToken, PasswordResetTokenExpiration = @PasswordResetTokenExpiration
                    WHERE Id = @Id"
            };

            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
            command.Parameters.AddWithValue("@PasswordResetToken", user.PasswordResetToken);
            command.Parameters.AddWithValue("@PasswordResetTokenExpiration", user.PasswordResetTokenExpiration);
            command.Parameters.AddWithValue("@Id", user.Id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
