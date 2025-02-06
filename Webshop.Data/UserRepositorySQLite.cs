using System.Data.SQLite;
using Webshop.Data.Models;

namespace Webshop.Data
{
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
                    CreatedAt DATETIME NOT NULL
                )"
            };

            command.ExecuteNonQuery();
        }

        public User Add(User newUser)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // **Check if user already exists**
            using var checkCommand = new SQLiteCommand("SELECT COUNT(1) FROM Users Where Email = @Email", connection);
            checkCommand.Parameters.AddWithValue("@Email", newUser.Email);

            if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
            {
                throw new InvalidOperationException("Email already registered.");
            }

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

        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = new SQLiteCommand("SELECT * FROM Users", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                };

                users.Add(user);
            }

            return users;
        }

        public User? GetUserByEmail(string email)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = new SQLiteCommand("SELECT * FROM Users WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                };
            }

            return null;
        }
    }
}
