using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.Bim.DocumentManagement.Tdms.Helpers
{
    class VolumeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            List<(string guid, string constractor, float volume)> progress = item["Progress"].ToObject<List<(string guid, string constractor, float volume)>>(serializer);
            float plan = item["Plan"].Value<float>();
            float fact = item["Fact"].Value<float>();

            return new Volume() { Plan = plan, Fact = fact, Progress = progress };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            t.WriteTo(writer);
        }
    }
}
