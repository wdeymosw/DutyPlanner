using System.IO;
using System.Text.Json;

namespace DutyPlanner.Infrastructure.JsonFileStorage
{
    internal class JsonFileStorage : IJsonFileStorage
    {

        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };

        public bool Exists(string path) => File.Exists(path);

        public T Load<T>(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public void Save<T>(string path, T data)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(path, json);
        }
    }
}
