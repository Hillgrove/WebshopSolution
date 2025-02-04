using Webshop.Data.Models;

namespace Webshop.Services
{
    public class UserService
    {
        private readonly HashingService _hashingService;

        public UserService(HashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public User CreateUser(string email, string password)
        {
            var createdUser = new User
            {
                Email = email,
                PasswordHash = _hashingService.GenerateHash(password)
            };

            return createdUser;
        }
    }
}
