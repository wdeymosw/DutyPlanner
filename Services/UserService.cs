using DutyPlanner.Domain.Repositories;
using DutyPlanner.Models;

namespace DutyPlanner.Services
{
    internal class UserService : IUserService
    {
        private readonly List<User> _users = new();
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
            _users.AddRange(_repository.GetAll());
        }

        public IReadOnlyList<User> GetAll() => _users;

        public User Add(string name, int hours)
        {
            var user = new User(Guid.NewGuid(), name, hours);
            _users.Add(user);
            _repository.Save(_users);
            return user;
        }

        public void Update(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index < 0) return;

            _users[index] = new User(user.Id, user.Name, user.Hours);
            _repository.Save(_users);
        }

        public void Remove(Guid Id)
        {
            var index = _users.FindIndex(u => u.Id == Id);
            if (index < 0) return;

            _users.RemoveAt(index);
            _repository.Save(_users);
        }
    }
}
