using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class Vector3Vector3LowercaseConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Vector3?)value;
            if (!vector.HasValue)
                return;

            var jo = JToken.FromObject(
                new
                {
                    x = vector.Value.X,
                    y = vector.Value.Y,
                    z = vector.Value.Z,
                });
            jo.WriteTo(writer);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            Vector3? myCustomType = null;

            if (reader.TokenType != JsonToken.Null)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var token = JToken.Load(reader);
                    myCustomType = new Vector3(
                        token.Value<float>("x"),
                        token.Value<float>("y"),
                        token.Value<float>("z"));
                }
            }

            return myCustomType;
        }

        public override bool CanConvert(Type objectType)
            => objectType == typeof(Vector3?);
    }
}
