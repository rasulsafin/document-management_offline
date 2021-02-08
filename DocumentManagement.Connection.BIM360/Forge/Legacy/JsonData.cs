using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Forge.Legacy
{
    [DataContract]
    public class JsonData<T> where T : class
    {
        [DataMember]
        public T data;
        [DataMember(EmitDefaultValue = false)]
        public T results;
        [DataMember]
        public Meta meta;

        private const string BASE_URL = "https://developer.api.autodesk.com";
        public static RestClient client = new RestClient(BASE_URL);

        protected static async Task<string> GetResponse(Method methodType, string apiString,
            Dictionary<string, string> parameters, T data = null)
        {
            var request = new RestRequest(apiString, methodType);
            foreach (var item in parameters)
                request.AddParameter(item.Key, item.Value, ParameterType.UrlSegment);
            request.AddHeader("Authorization", "Bearer " + AuthenticationService.Instance.accessProperty.Token);
            if (data != null)
            {
                request.AddHeader("Content-Type", "application/vnd.api+json");
                var jsonData = new JsonData<T>
                {
                    data = data
                };
                request.AddParameter("text/json", JsonConvert.SerializeObject(jsonData), ParameterType.RequestBody);
            }
            var response = await client.ExecuteTaskAsync(request);
            if (!response.IsSuccessful)
                throw new Exception(response.Content);
            return response.Content;
        }

        public static async Task<JsonData<T>> GetDeserializedData(string apiString, Dictionary<string, string> parameters) 
            => JsonConvert.DeserializeObject<JsonData<T>>(await GetResponse(Method.GET, apiString, parameters));

        public static async Task<JsonData<T>> PostSerializedData(string apiString, Dictionary<string, string> parameters, T data)
        {
            await GetResponse(Method.POST, apiString, parameters, data);
            // Ignore the response.
            return null;
        }

        public static async Task<JsonData<T>> PatchSerializedData(string apiString, Dictionary<string, string> parameters, T data)
        {
            await GetResponse(Method.PATCH, apiString, parameters, data);
            // Ignore the response.
            return null;
        }

        [DataContract]
        public class Meta
        {
            [DataMember]
            public int record_count;
            [DataMember]
            public Page page;

            [DataContract]
            public class Page
            {
                [DataMember]
                public int offset;
                [DataMember]
                public int limit;
            }
        }
    }

    public class JsonData<T, T2> : JsonData<T> where T : class
    {
        [DataMember(EmitDefaultValue = false)]
        public T2 included;

        public new static async Task<JsonData<T, T2>> GetDeserializedData(string apiString, Dictionary<string, string> parameters)
            => JsonConvert.DeserializeObject<JsonData<T, T2>>(await GetResponse(Method.GET, apiString, parameters));
    }
}
