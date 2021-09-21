using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class MillisecondsUnixDateTimeConverter : UnixDateTimeConverter
    {
        private static readonly DateTime UNIX_EPOCH = new (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    return;

                case DateTime dateTime:
                    {
                        long seconds = (long)(dateTime.ToUniversalTime() - UNIX_EPOCH).TotalMilliseconds;
                        writer.WriteValue(seconds);
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
                return UNIX_EPOCH.AddSeconds(milliseconds / 1000);
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
