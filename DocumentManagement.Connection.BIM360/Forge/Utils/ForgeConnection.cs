using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge
{
    public class ForgeConnection : IDisposable
    {
        private const string MEDIA_TYPE_JSON = "text/json";
        private const string CONTENT_TYPE = "application/vnd.api+json";

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

        public async Task<Stream> GetResponseStreamAuthorizedAsync(
                HttpMethod methodType,
                string command,
                params object[] arguments)
        {
            using var request = CreateRequest(methodType, command, arguments, true);
            var response = await SendRequestAsync(request, completionOption: HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<JObject> SendAsync(
                ForgeSettings settings,
                string command,
                params object[] arguments)
        {
            using var request = CreateRequest(settings.MethodType, command, arguments, settings.IsAuthorized);

            if (settings.Content != null)
            {
                request.Content = settings.Content;
            }
            else if (settings.Stream != null)
            {
                if (settings.RangeHeaderValue != null)
                    request.Headers.Range = settings.RangeHeaderValue;
                request.Content = new StreamContent(settings.Stream);
            }
            else if (settings.Data != null)
            {
                object data;

                if (settings.NeedDataKey)
                {
                    var requestData = new RequestData();

                    if (settings.NeedJsonApi)
                        requestData.JsonApi = JsonApi.Default;
                    if (settings.Data != null)
                        requestData.Data = settings.Data;
                    if (settings.Included != null)
                        requestData.Included = settings.Included;

                    data = requestData;
                }
                else
                {
                    data = settings.Data;
                }

                var serializedData = JsonConvert.SerializeObject(data, jsonSerializerSettings);
                request.Content = new StringContent(serializedData,
                        Encoding.Default,
                        MEDIA_TYPE_JSON);

                request.Content.Headers.ContentType = new MediaTypeHeaderValue(CONTENT_TYPE);
            }

            return await GetResponseAsync(request);
        }

        private HttpRequestMessage CreateRequest(HttpMethod methodType, string command, object[] arguments, bool isAuthorized)
        {
            var uri = Resources.ForgeUrl + command;
            uri = string.Format(uri, arguments);
            var request = new HttpRequestMessage(methodType, uri);
            if (isAuthorized)
                request.Headers.Authorization = new AuthenticationHeaderValue(Constants.AUTHORIZATION_SCHEME, Token);
            return request;
        }

        private async Task<JObject> GetResponseAsync(HttpRequestMessage request, bool isAuthorized = true)
        {
            using var response = await SendRequestAsync(request, isAuthorized);
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(content);
            return jObject;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(
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
