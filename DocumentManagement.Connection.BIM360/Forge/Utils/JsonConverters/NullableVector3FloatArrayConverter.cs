using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class NullableVector3FloatArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Vector3?)value;
            if (!vector.HasValue)
                return;

            var jo = JToken.FromObject(new[] { vector.Value.X, vector.Value.Y, vector.Value.Z });
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
                    var items = token.ToObject<float[]>();
                    if (items == null || items.Length != 3)
                        throw new NotSupportedException("Supports three-dimensional vectors only");

                    myCustomType = new Vector3(items[0], items[1], items[2]);
                }
            }

            return myCustomType;
        }

        public override bool CanConvert(Type objectType)
            => objectType == typeof(Vector3?);
    }
}
