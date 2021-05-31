using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public abstract class ANullableVector3Converter : JsonConverter
    {
        private readonly JsonToken type;

        protected ANullableVector3Converter(JsonToken type)
            => this.type = type;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Vector3?)value;
            if (!vector.HasValue)
                return;

            var token = JToken.FromObject(ConvertVector3ToSerializingObject(vector.Value));
            token.WriteTo(writer);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            Vector3? result = null;

            if (reader.TokenType != JsonToken.Null)
            {
                if (reader.TokenType == type)
                {
                    var token = JToken.Load(reader);
                    result = ConvertTokenToVector3(token);
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
            => objectType == typeof(Vector3?);

        protected abstract Vector3 ConvertTokenToVector3(JToken token);

        protected abstract object ConvertVector3ToSerializingObject(Vector3 vector);
    }
}
