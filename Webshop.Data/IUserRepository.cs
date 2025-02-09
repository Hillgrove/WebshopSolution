using Webshop.Data.Models;

namespace Webshop.Data
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User newUser);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiration);
        Task<User?> GetUserByPasswordResetTokenAsync(string token);
        Task UpdateAsync(User user);
    }
}