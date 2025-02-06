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

            var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
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

            var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                    INSERT INTO Users (Email, PasswordHash, CreatedAt)
                    VALUES (@Email, @PasswordHash, @CreatedAt);
                    SELECT last_insert_rowid()"
            };

            command.Parameters.AddWithValue("@Email", newUser.Email);
            command.Parameters.AddWithValue("@PasswordHash", newUser.PasswordHash);
            command.Parameters.AddWithValue("CreatedAt", newUser.CreatedAt);

            newUser.Id = Convert.ToInt32(command.ExecuteScalar());

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
