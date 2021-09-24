using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class MillisecondsUnixDateTimeConverter : UnixDateTimeConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    return;

                case DateTime dateTime:
                    {
                        long milliseconds = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
                        writer.WriteValue(milliseconds);
                        break;
                    }

                case DateTimeOffset dateTimeOffset:
                    {
                        long milliseconds = dateTimeOffset.ToUnixTimeMilliseconds();
                        writer.WriteValue(milliseconds);
                        break;
                    }

                default:
                    base.WriteJson(writer, value, serializer);
                    break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                long milliseconds = (long)reader.Value!;

                var d = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

                if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
                    return d;

                return d.UtcDateTime;
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
