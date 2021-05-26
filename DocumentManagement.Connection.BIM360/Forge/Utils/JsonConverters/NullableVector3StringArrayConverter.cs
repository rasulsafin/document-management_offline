using System;
using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class NullableVector3StringArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Vector3?)value;
            if (!vector.HasValue)
                return;

            var jo = JToken.FromObject(
                new[]
                {
                    vector.Value.X.ToString(CultureInfo.InvariantCulture),
                    vector.Value.Y.ToString(CultureInfo.InvariantCulture),
                    vector.Value.Z.ToString(CultureInfo.InvariantCulture),
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
                if (reader.TokenType == JsonToken.StartArray)
                {
                    var token = JToken.Load(reader);
                    var items = token.ToObject<string[]>();
                    if (items == null || items.Length != 3)
                        throw new NotSupportedException("Supports three-dimensional vectors only");

                    if (float.TryParse(items[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                        float.TryParse(items[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y) &&
                        float.TryParse(items[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var z))
                        myCustomType = new Vector3(x, y, z);
                }
            }

            return myCustomType;
        }

        public override bool CanConvert(Type objectType)
            => objectType == typeof(Vector3?);
    }
}
