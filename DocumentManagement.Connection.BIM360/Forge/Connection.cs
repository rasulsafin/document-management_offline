using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocumentManagement.Connection.BIM360.Forge
{
    public class Connection : IDisposable
    {
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
            using var request = CreateRequest(methodType, command, arguments);

            if (data != null)
            {
                var jsonData = new { data };
                request.Content =
                        new StringContent(JsonConvert.SerializeObject(jsonData), Encoding.Default, MEDIA_TYPE_JSON);
            }

            return await SendAsync(request);
        }

        public async Task<JObject> SendRequestWithStream(HttpMethod methodType, string command, Stream data, RangeHeaderValue rangeHeaderValue, params object[] arguments)
        {
            // TODO: check is it working or not?
            using var request = CreateRequest(methodType, command, arguments);

            if (data != null)
            {
                if (rangeHeaderValue != null)
                    request.Headers.Range = rangeHeaderValue;
                request.Content = new StreamContent(data);
            }

            return await SendAsync(request);

        }

        private HttpRequestMessage CreateRequest(HttpMethod methodType, string command, object[] arguments)
        {
            var uri = Resources.ForgeUrl + command;
            uri = string.Format(uri, arguments);
            var request = new HttpRequestMessage(methodType, uri);
            return request;
        }

        private async Task<JObject> SendAsync(HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(content);
            return jObject;
        }
    }
}
