using DutyPlanner.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DutyPlanner.Infrastructure.JsonFileStorage
{
    public class DayUserListConverter : JsonConverter<List<DayUserDto>>
    {
        public override List<DayUserDto> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"Expected array, got {reader.TokenType}");

            var list = new List<DayUserDto>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Создаём временный converter для каждого элемента
                    var converter = new DayUserDtoConverter();
                    var item = converter.Read(ref reader, typeof(DayUserDto), options);
                    list.Add(item);
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<DayUserDto> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            var converter = new DayUserDtoConverter();
            foreach (var item in value)
            {
                converter.Write(writer, item, options);
            }
            writer.WriteEndArray();
        }
    }
}
