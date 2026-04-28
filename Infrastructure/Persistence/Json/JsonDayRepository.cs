using DutyPlanner.Domain.Repositories;
using DutyPlanner.Infrastructure.JsonFileStorage;
using DutyPlanner.Models;
using System.IO;
using System.Text.Json;

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

        public DayFileDto Load(string filePath)
        {
            if (!_storage.Exists(filePath))
                return new DayFileDto();

            var raw = File.ReadAllText(filePath).TrimStart();
            if (raw.StartsWith('['))
                return new DayFileDto { Users = JsonSerializer.Deserialize<List<DayUserDto>>(raw) ?? [] };

            return _storage.Load<DayFileDto>(filePath);
        }

        public void Save(string filePath, DayFileDto data)
        {
            _storage.Save(filePath, data);
        }

        public void InitializeDay(string filePath)
        {
            if (!_storage.Exists(filePath))
                _storage.Save(filePath, new DayFileDto());
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

        public async Task<DayFileDto> LoadAsync(string filePath)
        {
            if (!_storage.Exists(filePath))
                return new DayFileDto();

            var raw = (await File.ReadAllTextAsync(filePath)).TrimStart();
            if (raw.StartsWith('['))
                return new DayFileDto { Users = JsonSerializer.Deserialize<List<DayUserDto>>(raw) ?? [] };

            return await _storage.LoadAsync<DayFileDto>(filePath);
        }

        public Task SaveAsync(string filePath, DayFileDto data)
            => _storage.SaveAsync(filePath, data);
    }
}
