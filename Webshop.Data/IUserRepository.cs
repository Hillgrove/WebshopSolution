using Webshop.Data.Models;

namespace Webshop.Data
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddAsync(User newUser);
        Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiration);
        Task<User?> GetUserByPasswordResetTokenAsync(string token);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);

    }
}