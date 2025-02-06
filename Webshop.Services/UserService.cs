using Webshop.Data;
using Webshop.Data.Models;

namespace Webshop.Services
{
    public class UserService
    {
        private readonly HashingService _hashingService;
        private readonly IUserRepository _userRepository;

        public UserService(HashingService hashingService, IUserRepository userRepository)
        {
            _hashingService = hashingService;
            _userRepository = userRepository;
        }

        public User CreateUser(string email, string password)
        {
            var passwordHash = _hashingService.GenerateHash(password);

            return new User
            {
                Email = email,
                PasswordHash = passwordHash
            };
        }

        public bool VerifyUserCredentials(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return false;
            }

            return _hashingService.VerifyHash(password, user.PasswordHash);
        }
    }
}
