using Webshop.Data.Models;

namespace Webshop.Data
{
    public class UserRepositoryList : IUserRepository
    {
        private int _nextId = 1;
        private readonly List<User> _users = new();

        public UserRepositoryList() { }

        public IEnumerable<User> GetAll()
        {
            return new List<User>(_users);
        }

        public User? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email == email);
        }

        public User Add(User newUser)
        {
            if (GetUserByEmail(newUser.Email) != null)
            {
                throw new InvalidOperationException("Email already registered.");
            }

            // TODO: Should I add validation?
            newUser.Id = _nextId++;
            _users.Add(newUser);
            return newUser;
        }
    }
}
