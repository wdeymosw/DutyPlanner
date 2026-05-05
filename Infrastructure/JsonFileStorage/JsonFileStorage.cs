using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DutyPlanner.Models;

namespace DutyPlanner.Infrastructure.JsonFileStorage
{
    internal class JsonFileStorage : IJsonFileStorage
    {

        private readonly JsonSerializerOptions _options;

        public JsonFileStorage()
        {
            _options = CreateOptions();
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Добавляем converters
            options.Converters.Add(new DayUserDtoConverter());
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

            return options;
        }

        public bool Exists(string path) => File.Exists(path);

        public T Load<T>(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            var json = File.ReadAllText(path);
            try
            {
                return JsonSerializer.Deserialize<T>(json, _options)!;
            }
            catch (JsonException ex)
            {
                var errorMsg = $"Ошибка десериализации {typeof(T).Name} из файла {path}.\n" +
                    $"JSON содержимое (первые 500 символов):\n{json[..Math.Min(500, json.Length)]}\n" +
                    $"Полная ошибка: {ex.Message}";
                throw new JsonException(errorMsg, ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при загрузке {path}: {ex.Message}", ex);
            }
        }

        public void Save<T>(string path, T data)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(path, json);
        }

        public async Task<T> LoadAsync<T>(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            var json = await File.ReadAllTextAsync(path);
            try
            {
                return JsonSerializer.Deserialize<T>(json, _options)!;
            }
            catch (JsonException ex)
            {
                var errorMsg = $"Ошибка десериализации {typeof(T).Name} из файла {path}.\n" +
                    $"JSON содержимое (первые 500 символов):\n{json[..Math.Min(500, json.Length)]}\n" +
                    $"Полная ошибка: {ex.Message}";
                throw new JsonException(errorMsg, ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при загрузке {path}: {ex.Message}", ex);
            }
        }

        public async Task SaveAsync<T>(string path, T data)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(data, _options);
            await File.WriteAllTextAsync(path, json);
        }
    }
}
