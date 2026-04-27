using DutyPlanner.Infrastrustures.JsonFileStorage;
using DutyPlanner.Models;
using System.IO;
using System.Text.Json;

namespace DutyPlanner.Services
{
    internal class UserService : IUserService
    {

        private readonly List<User> _users = new();

        private readonly IJsonFileStorage _fileStorage;
        private readonly string _filePath;

       



        public UserService(IJsonFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
            _filePath = Path.Combine("Data", "Users", "users.json");
            Load();
        }


        public IReadOnlyList<User> GetAll() => _users;


        //public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

        public User Add(string name, int hours)
        {
            var user = new User(Guid.NewGuid(), name, hours);
            _users.Add(user);
            Save();
            return user;
        }


        public void Update(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index < 0) return;

            _users[index] = new User(user.Id, user.Name, user.Hours);
            Save();

        }


        public void Remove(Guid Id)
        {
            var index = _users.FindIndex(u => u.Id == Id);
            if (index < 0) return;

            _users.RemoveAt(index);
            Save();
        }


        private void Load()
        {
            if (!File.Exists(_filePath)) return;
            var data = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(_filePath));
            if (data != null) _users.AddRange(data);
        }

        public void Save()
        {
            _fileStorage.Save(_filePath, _users);
        }

      



    }
}
