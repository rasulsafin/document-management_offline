using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Utils
{
    public class HttpConnection : IDisposable
    {
        protected readonly HttpClient client;
        protected readonly JsonSerializerSettings jsonSerializerSettings =
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public HttpConnection()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(Timeout) };

        protected double Timeout { get; set; } = 10;

        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<Stream> GetResponseStreamAuthorizedAsync(
                HttpMethod methodType,
                string command,
                (string scheme, string token) authData = default,
                params object[] arguments)
        {
            using var request = CreateRequest(methodType, command, arguments, authData);
            var response = await SendRequestAsync(request, completionOption: HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync();
        }

        public virtual HttpRequestMessage CreateRequest(
                HttpMethod methodType,
                string uri,
                object[] arguments = null,
                (string scheme, string token) authData = default)
        {
            var argumentsArray = arguments ?? Array.Empty<object>();
            var formattedUri = string.Format(uri, argumentsArray);
            var request = new HttpRequestMessage(methodType, formattedUri);
            if (authData != default)
                request.Headers.Authorization = new AuthenticationHeaderValue(authData.scheme, authData.token);

            return request;
        }

        public async Task<JObject> GetResponseAsync(
                HttpRequestMessage request,
                (string scheme, string token) authData = default)
        {
            Exception exception = null;
            string result = string.Empty;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using var response = await SendRequestAsync(request, authData);
                    result = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(result);
                    response.EnsureSuccessStatusCode();
                    return jObject;
                }
                catch (Exception e)
                {
                    exception = e;
                    await Task.Delay(1000);
                }
            }

            throw new Exception(result, exception);
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
                HttpRequestMessage request,
                (string scheme, string token) authData = default,
                HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            if (authData != default)
                request.Headers.Authorization = new AuthenticationHeaderValue(authData.scheme, authData.token);
            var response = await client.SendAsync(request, completionOption);
            return response;
        }
    }
}
