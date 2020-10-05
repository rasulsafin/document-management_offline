using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement.Tdms.Helpers
{
    class OperationJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            List<dynamic> operations = array.ToObject<List<dynamic>>();

            for (int i = 0; i < operations.Count; i++)
            {
                operations[i] = (operations[i] as JObject).ToObject<(bool, string, string)>();
            }

            return operations;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            t.WriteTo(writer);
        }
    }
}