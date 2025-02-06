using Webshop.Data.Models;

namespace Webshop.Data
{
    public interface IUserRepository
    {
        User Add(User newUser);
        IEnumerable<User> GetAll();
        User? GetUserByEmail(string email);
    }
}