using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.LementPro
{
    public class NetConnector : IDisposable
    {
        private const string CONTENT_TYPE = "application/json; charset=UTF-8";

        private static readonly double TIMEOUT = 10;
        private static readonly string AUTHENTICATION_SCHEME = "auth";

        private readonly HttpClient client;

        public NetConnector()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(TIMEOUT) };

        public void Dispose()
            => client.Dispose();

        public HttpRequestMessage CreateRequest(
                HttpMethod methodType,
                string uri,
                object[] arguments = null,
                string token = null)
        {
            var argumentsArray = arguments ?? Array.Empty<object>();
            var formattedUri = string.Format(uri, argumentsArray);
            var request = new HttpRequestMessage(methodType, formattedUri);
            if (token != null)
                request.Headers.Authorization = new AuthenticationHeaderValue(AUTHENTICATION_SCHEME, token);

            return request;
        }

        public async Task<JObject> GetResponseAsync(
                HttpRequestMessage request,
                string token)
        {
            using var response = await SendRequestAsync(request, token);
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(content);
            return jObject;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
                HttpRequestMessage request,
                string token = null,
                HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            if (token != null)
                request.Headers.Authorization = new AuthenticationHeaderValue(AUTHENTICATION_SCHEME, token);
            var response = await client.SendAsync(request, completionOption);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
