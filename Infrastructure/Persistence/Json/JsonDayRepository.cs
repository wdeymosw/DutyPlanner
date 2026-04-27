using DutyPlanner.Domain.Repositories;
using DutyPlanner.Infrastrustures.JsonFileStorage;
using DutyPlanner.Models;

namespace DutyPlanner.Infrastructure.Persistence.Json
{
    internal class JsonDayRepository : IDayRepository
    {
        private readonly IJsonFileStorage _storage;

        public JsonDayRepository(IJsonFileStorage storage)
        {
            _storage = storage;
        }

        public bool Exists(string filePath) => _storage.Exists(filePath);

        public List<DayUserDto> Load(string filePath)
        {
            if (!_storage.Exists(filePath))
                return [];

            return _storage.Load<List<DayUserDto>>(filePath);
        }

        public void Save(string filePath, IEnumerable<DayUserDto> users)
        {
            _storage.Save(filePath, users.ToList());
        }
    }
}
