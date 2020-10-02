using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MRS.Bim.DocumentManagement;
using MRS.Bim.DocumentManagement.Tdms;

namespace MRS.Bim.DocumentManagement.Tdms.Helpers
{
    class JSONObject
    {
        [JsonProperty]
        private string name;
        [JsonProperty]
        private string type;
        [JsonProperty]
        private string json;

        private static readonly Lazy<Dictionary<string, Type>> types = new Lazy<Dictionary<string, Type>>(DetectTypes);

        public JSONObject() { }

        public JSONObject(string name, string type, string json)
        {
            this.name = name;
            this.type = type;
            this.json = json;
        }

        public Tuple<string, object> ConvertFromJson(string data)
        {
            JSONObject obj = JsonConvert.DeserializeObject<JSONObject>(data);

            object input = ((Func<string, bool>)JsonConvert.DeserializeObject<bool>)
                .Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(types.Value[obj.type])
                .Invoke(this, new object[] { obj.json });

            return new Tuple<string, object>(obj.name, input);
        }

        public string ConvertToJson(string meth, string type, object obj)
        {
            string output = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All
            });
            JSONObject dataJSON = new JSONObject(meth, type, output);

            return JsonConvert.SerializeObject(dataJSON);
        }

        public T ConvertJObject<T>(JObject o)
        {
            JsonSerializer serializer = new JsonSerializer();
            return (T)serializer.Deserialize(new JTokenReader(o), typeof(T));
        }

        private static Dictionary<string, Type> DetectTypes()
        {
            var listOfTypes = new List<Type>
            {
                typeof(bool),
                typeof(string),
                typeof(float),
                typeof(DMFile),
                typeof(DMFile[]),
                typeof(Volume),
                typeof(Volume[]),
                typeof(Issue),
                typeof(Issue[]),
                typeof(Job),
                typeof(Job[]),
                typeof(Project),
                typeof(Project[]),
                typeof((bool, string)),
                typeof((string, DMFileType)),
                typeof((string, DMFile)),
                typeof((string, DMFile[])),
                typeof((string, DMAction)),
                typeof((string, DMAction[])),
                typeof((string, string, string, string)),
                typeof(Dictionary<string, DMItem[]>),
            };

            var dictionary = new Dictionary<string, Type>();
            listOfTypes.ForEach(x => dictionary.Add(x.ToShortString(), x));
            
            return dictionary;
        }
    }
}
