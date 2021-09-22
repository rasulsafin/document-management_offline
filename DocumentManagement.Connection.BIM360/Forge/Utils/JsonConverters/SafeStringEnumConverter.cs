using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    // https://stackoverflow.com/a/61685864
    public class SafeStringEnumConverter : StringEnumConverter
    {
        public SafeStringEnumConverter(object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch
            {
                return DefaultValue;
            }
        }
    }
}
