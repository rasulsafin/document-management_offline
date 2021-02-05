using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocumentManagement.Connection.BIM360.Forge
{
    public class Connection : IDisposable
    {
        private const string CONTENT_TYPE_HEADER = "Content-Type";
        private const string CONTENT_TYPE_JSON = "application/vnd.api+json";
        private const string MEDIA_TYPE_JSON = "text/json";

        private static readonly double TIMEOUT = 10;

        private readonly HttpClient client;

        public Connection()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(TIMEOUT) };

        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<JObject> GetResponse(HttpMethod methodType, string command, params object[] arguments)
            => await SendRequestWithSerializedData(methodType, command, (object)null, arguments);

        public async Task<JObject> SendRequestWithSerializedData<T>(HttpMethod methodType, string command, T data, params object[] arguments)
        {
            var uri = Resources.ForgeUrl + command;
            uri = string.Format(uri, arguments);
            var request = new HttpRequestMessage(methodType, uri);

            if (data != null)
            {
                request.Headers.Add(CONTENT_TYPE_HEADER, CONTENT_TYPE_JSON);
                var jsonData = new { data };
                request.Content =
                        new StringContent(JsonConvert.SerializeObject(jsonData), Encoding.Default, MEDIA_TYPE_JSON);
            }

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(content);
            return jObject;
        }
    }
}
