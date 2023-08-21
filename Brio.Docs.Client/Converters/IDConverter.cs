using System;
using Brio.Docs.Client.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Client.Converters
{
    public class IDConverter<T> : JsonConverter<ID<T>>
    {
        public override ID<T> ReadJson(JsonReader reader, Type objectType, ID<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var idObject = JObject.Load(reader);

                if (idObject.TryGetValue("id", out var idToken) && idToken.Type == JTokenType.Integer)
                {
                    var idValue = (int)idToken;
                    var id = new ID<T>(idValue); // Replace with the actual constructor of your ID type.
                    return id;
                }
            }

            if (reader.TokenType == JsonToken.Integer)
            {
                var idValue = (long)reader.Value;

                // Create an instance of your ID<T> type using the parsed idValue.
                var id = new ID<T>((int)idValue); // Replace with the actual constructor of your ID type.
                return id;
            }

            // Hard Code
            if (reader.TokenType == JsonToken.EndObject)
            {
                var id = new ID<T>(0);
                return id;
            }

            throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
        }

        public override void WriteJson(JsonWriter writer, ID<T> value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(((int)value)); // Assuming ID<ProjectDto>.Value returns the underlying int value.
        }
    }
}
