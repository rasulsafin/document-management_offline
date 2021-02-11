using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge
{
    public class ForgeConnection : IDisposable
    {
        private const string MEDIA_TYPE_JSON = "text/json";

        private static readonly double TIMEOUT = 10;

        private readonly HttpClient client;
        private readonly JsonSerializerSettings jsonSerializerSettings =
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public ForgeConnection()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(TIMEOUT) };

        public string Token { get; set; }

        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Send the authorized request without a content to Forge server.
        /// </summary>
        /// <param name="methodType">Type of standard HTTP methods(GET, POST etc.).</param>
        /// <param name="command">Route of Forge method with format items if needed.</param>
        /// <param name="arguments">Inserting objects to command line.</param>
        /// <returns>Deserialized response from Forge.</returns>
        public async Task<JObject> GetResponseAuthorizedAsync(
                HttpMethod methodType,
                string command,
                params object[] arguments)
            => await SendSerializedDataAuthorizedAsync(methodType, command, (object)null, arguments);

        public async Task<Stream> GetResponseStreamAuthorizedAsync(
                HttpMethod methodType,
                string command,
                params object[] arguments)
        {
            using var request = CreateRequest(methodType, command, arguments);
            var response = await SendAsync(request, completionOption: HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Send the authorized request with a JSON content to Forge server.
        /// </summary>
        /// <param name="methodType">Type of standard HTTP methods(GET, POST etc.).</param>
        /// <param name="command">Route of Forge method with format items if needed.</param>
        /// <param name="data">Sending serializing data that be annotated by "data" in JSON.</param>
        /// <param name="arguments">Inserting objects to command line.</param>
        /// <typeparam name="T">Type of sending data. Must be DataContract.</typeparam>
        /// <returns>Deserialized response from Forge.</returns>
        public async Task<JObject> SendSerializedDataAuthorizedAsync<T>(
                HttpMethod methodType,
                string command,
                T data,
                params object[] arguments)
        {
            using var request = CreateRequest(methodType, command, arguments);

            if (data != null)
            {
                var jsonData = new { data };
                request.Content = new StringContent(JsonConvert.SerializeObject(jsonData, jsonSerializerSettings),
                        Encoding.Default,
                        MEDIA_TYPE_JSON);
            }

            return await GetResponseAsync(request);
        }

        /// <summary>
        /// Send the authorized request with a stream content to Forge server.
        /// </summary>
        /// <param name="methodType">Type of standard HTTP methods(GET, POST etc.).</param>
        /// <param name="command">Route of Forge method with format items if needed.</param>
        /// <param name="data">Sending stream.</param>
        /// <param name="rangeHeaderValue">Header of ranges if needed.</param>
        /// <param name="arguments">Inserting objects to command line.</param>
        /// <returns>Deserialized response from Forge.</returns>
        public async Task<JObject> SendStreamAuthorizedAsync(
                HttpMethod methodType,
                string command,
                Stream data,
                RangeHeaderValue rangeHeaderValue,
                params object[] arguments)
        {
            // TODO: check is it working or not?
            using var request = CreateRequest(methodType, command, arguments);

            if (data != null)
            {
                if (rangeHeaderValue != null)
                    request.Headers.Range = rangeHeaderValue;
                request.Content = new StreamContent(data);
            }

            return await GetResponseAsync(request);
        }

        /// <summary>
        /// Send the request with a content to Forge server.
        /// </summary>
        /// <param name="methodType">Type of standard HTTP methods(GET, POST etc.).</param>
        /// <param name="command">Route of the Forge method with format items if needed.</param>
        /// <param name="content">Sending data.</param>
        /// <param name="isAuthorized">Is need the add authentication header.</param>
        /// <param name="arguments">Inserting objects to command line.</param>
        /// <returns>Deserialized the response from Forge.</returns>
        public async Task<JObject> SendRequestAsync(
                HttpMethod methodType,
                string command,
                HttpContent content,
                bool isAuthorized,
                params object[] arguments)
        {
            using var request = CreateRequest(methodType, command, arguments);
            if (content != null)
                request.Content = content;
            return await GetResponseAsync(request, isAuthorized);
        }

        private HttpRequestMessage CreateRequest(HttpMethod methodType, string command, object[] arguments)
        {
            var uri = Resources.ForgeUrl + command;
            uri = string.Format(uri, arguments);
            var request = new HttpRequestMessage(methodType, uri);
            return request;
        }

        private async Task<JObject> GetResponseAsync(HttpRequestMessage request, bool isAuthorized = true)
        {
            var response = await SendAsync(request, isAuthorized);
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(content);
            return jObject;
        }

        private async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                bool isAuthorized = true,
                HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            if (isAuthorized)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var response = await client.SendAsync(request, completionOption);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
