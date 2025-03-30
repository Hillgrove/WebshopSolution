using Webshop.Data.Models;

namespace Webshop.Data
{
    public class UserRepositoryList : IUserRepository
    {
        private int _nextId = 1;
        private readonly List<User> _users = new();

        public UserRepositoryList() { }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<User>>(new List<User>(_users));
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        }

        public Task<User> AddAsync(User newUser)
        {
            newUser.Id = _nextId++;
            _users.Add(newUser);
            return Task.FromResult(newUser);
        }

        public Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiration)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiration = expiration;
            }
            return Task.CompletedTask;
        }

        public Task<User?> GetUserByPasswordResetTokenAsync(string token)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.PasswordResetToken == token));
        }

        public Task UpdateAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.Email = user.Email;
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.PasswordResetToken = user.PasswordResetToken;
                existingUser.PasswordResetTokenExpiration = user.PasswordResetTokenExpiration;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
