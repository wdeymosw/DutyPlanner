using DutyPlanner.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DutyPlanner.Infrastructure.JsonFileStorage
{
    public class DayUserDtoConverter : JsonConverter<DayUserDto>
    {
        public override DayUserDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected StartObject, got {reader.TokenType}");

            Guid instanceId = Guid.Empty;
            Guid userId = Guid.Empty;
            string? name = null;
            int hours = 0;
            DayUserPlacement placement = DayUserPlacement.Active;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new DayUserDto(userId, name, hours, placement)
                    {
                        InstanceId = instanceId
                    };
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                string propertyName = reader.GetString()?.ToLowerInvariant() ?? "";
                reader.Read();

                switch (propertyName)
                {
                    case "instanceid":
                        instanceId = reader.GetGuid();
                        break;
                    case "userid":
                        userId = reader.GetGuid();
                        break;
                    case "name":
                        name = reader.GetString();
                        break;
                    case "hours":
                        hours = reader.GetInt32();
                        break;
                    case "placement":
                        // Поддерживаем оба варианта: строка и число
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            string placementStr = reader.GetString() ?? "Active";
                            placement = Enum.TryParse<DayUserPlacement>(placementStr, ignoreCase: true, out var p)
                                ? p
                                : DayUserPlacement.Active;
                        }
                        else if (reader.TokenType == JsonTokenType.Number)
                        {
                            int placementNum = reader.GetInt32();
                            placement = Enum.IsDefined(typeof(DayUserPlacement), placementNum)
                                ? (DayUserPlacement)placementNum
                                : DayUserPlacement.Active;
                        }
                        break;
                }
            }

            throw new JsonException("Unexpected end of JSON");
        }

        public override void Write(Utf8JsonWriter writer, DayUserDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("instanceId", value.InstanceId);
            writer.WriteString("userId", value.UserID);
            writer.WriteString("name", value.Name);
            writer.WriteNumber("hours", value.Hours);
            writer.WriteString("placement", value.Placement.ToString());
            writer.WriteEndObject();
        }
    }
}
