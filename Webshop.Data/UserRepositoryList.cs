using Webshop.Data.Models;

namespace Webshop.Data
{
    public class UserRepositoryList
    {
        private int _nextId = 1;
        private readonly List<User> _users = new();

        public UserRepositoryList() { }

        public IEnumerable<User> GetAll()
        {
            return new List<User>(_users);
        }

        public User Add(User newUser)
        {
            // TODO: Should I add validation?
            newUser.Id = _nextId++;
            _users.Add(newUser);
            return newUser;
        }
    }
}
