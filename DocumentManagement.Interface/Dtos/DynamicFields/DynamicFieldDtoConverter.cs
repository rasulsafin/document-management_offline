using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DynamicFieldDtoConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDynamicFieldDto);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var jObject = JObject.Load(reader);

            if (jObject["type"] != null)
            {
                var type = jObject["type"].ToObject<DynamicFieldType>();

                switch (type)
                {
                    case DynamicFieldType.OBJECT:
                        return jObject.ToObject(typeof(DynamicFieldDto), serializer);
                    case DynamicFieldType.BOOL:
                        return jObject.ToObject(typeof(BoolFieldDto), serializer);
                    case DynamicFieldType.STRING:
                        return jObject.ToObject(typeof(StringFieldDto), serializer);
                    case DynamicFieldType.INTEGER:
                        return jObject.ToObject(typeof(IntFieldDto), serializer);
                    case DynamicFieldType.FLOAT:
                        return jObject.ToObject(typeof(FloatFieldDto), serializer);
                    case DynamicFieldType.DATE:
                        return jObject.ToObject(typeof(DateFieldDto), serializer);
                    case DynamicFieldType.ENUM:
                        return jObject.ToObject(typeof(EnumerationFieldDto), serializer);
                    default:
                        throw new NotImplementedException();
                }
            }

            throw new NullReferenceException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(IDynamicFieldDto));
        }
    }
}
