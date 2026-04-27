using DutyPlanner.Domain.Repositories;
using DutyPlanner.Infrastructure.JsonFileStorage;
using DutyPlanner.Models;
using System.IO;

namespace DutyPlanner.Infrastructure.Persistence.Json
{
    internal class JsonUserRepository : IUserRepository
    {
        private readonly IJsonFileStorage _storage;
        private readonly string _filePath = Path.Combine("Data", "Users", "users.json");

        public JsonUserRepository(IJsonFileStorage storage)
        {
            _storage = storage;
        }

        public IReadOnlyList<User> GetAll()
        {
            if (!_storage.Exists(_filePath))
                return [];

            return _storage.Load<List<User>>(_filePath);
        }

        public void Save(IEnumerable<User> users)
        {
            _storage.Save(_filePath, users.ToList());
        }
    }
}
