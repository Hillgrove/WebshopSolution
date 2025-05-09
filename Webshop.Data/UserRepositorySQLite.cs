﻿using System.Data.SQLite;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public class UserRepositorySQLite : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepositorySQLite(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> AddAsync(User newUser)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // Insert new user
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var insertCommand = new SQLiteCommand(connection)
                {
                    CommandText = @"
                    INSERT INTO Users (Email, Role, PasswordHash, CreatedAt)
                    VALUES (@Email, @Role, @PasswordHash, @CreatedAt);
                    SELECT last_insert_rowid()"
                };

                insertCommand.Parameters.AddWithValue("@Email", newUser.Email);
                insertCommand.Parameters.AddWithValue("@Role", "Customer");
                insertCommand.Parameters.AddWithValue("@PasswordHash", newUser.PasswordHash);
                insertCommand.Parameters.AddWithValue("@CreatedAt", newUser.CreatedAt);

                var result = await insertCommand.ExecuteScalarAsync();
                if (result == null)
                {
                    throw new Exception("Database operation failed: No ID was returned.");
                }

                newUser.Id = Convert.ToInt32(result);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to insert user", ex);
            }
            
            return newUser;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = new List<User>();

            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("SELECT * FROM Users", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    Role = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    PasswordResetToken = reader.IsDBNull(5) ? null : reader.GetString(5),
                    PasswordResetTokenExpiration = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
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
                    Role = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    PasswordResetToken = reader.IsDBNull(5) ? null : reader.GetString(5),
                    PasswordResetTokenExpiration = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
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
                    Role = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    PasswordResetToken = reader.IsDBNull(5) ? null : reader.GetString(5),
                    PasswordResetTokenExpiration = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
                };
            }

            return null;
        }
        
        public async Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiration)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            
            // TODO: add transactions to all db changes
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
                    Role = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    PasswordResetToken = reader.IsDBNull(5) ? null : reader.GetString(5),
                    PasswordResetTokenExpiration = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
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

        public async Task DeleteAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand("DELETE FROM Users WHERE ID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }

    }
}
