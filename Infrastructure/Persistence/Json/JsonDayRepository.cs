using DutyPlanner.Domain.Repositories;
using DutyPlanner.Infrastrustures.JsonFileStorage;
using DutyPlanner.Models;
using System.IO;

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

        public void InitializeDay(string filePath)
        {
            if (!_storage.Exists(filePath))
                _storage.Save(filePath, new List<DayUserDto>());
        }

        public void Delete(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public string[] GetDayFilePaths(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return [];

            return Directory.GetFiles(folderPath, "*.json");
        }

        public void EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
    }
}
