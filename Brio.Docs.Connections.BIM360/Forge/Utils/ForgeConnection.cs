using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.External;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge
{
    public class ForgeConnection : HttpConnection
    {
        internal static readonly TimeSpan MIN_TOKEN_LIFE = TimeSpan.FromMinutes(3);

        private const string MEDIA_TYPE_JSON = "text/json";
        private const string CONTENT_TYPE = "application/vnd.api+json";

        public ForgeConnection()
            => client.Timeout = TimeSpan.FromSeconds(30);

        public Func<string> GetToken { internal get; set; }

        public Func<string> GetAppToken { internal get; set; }

        public static string SetParameter(string uri, IQueryParameter filter)
        {
            var stringBuilder = new StringBuilder(uri);

            if (!uri.Contains('?'))
                stringBuilder.Append('?');
            if (stringBuilder[^1] != '&' && stringBuilder[^1] != '?')
                stringBuilder.Append('&');
            stringBuilder.AppendFormat(filter.ToQueryString());

            if (stringBuilder.Length > 0 && stringBuilder[^1] == '&')
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }

        public static string SetParameters(string uri, IEnumerable<IQueryParameter> filters = null)
        {
            var stringBuilder = new StringBuilder(uri);
            if (!uri.Contains('?'))
                stringBuilder.Append('?');

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (stringBuilder[^1] != '&' && stringBuilder[^1] != '?')
                        stringBuilder.Append('&');
                    stringBuilder.AppendFormat(filter.ToQueryString());
                }
            }

            if (stringBuilder.Length > 0 && stringBuilder[^1] == '&')
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }

        public async Task<Stream> GetResponseStreamAuthorizedAsync(
            HttpMethod methodType,
            string command,
            params object[] arguments)
            => await GetResponseStreamAuthorizedAsync(
                methodType,
                command,
                authData: (Constants.AUTHORIZATION_SCHEME, GetToken()),
                arguments);

        public async Task<JToken> SendAsync(
            ForgeSettings settings,
            string command,
            params object[] arguments)
        {
            return await GetResponseAsync(() => CreateHttpRequestMessage(settings, command, arguments));
        }

        public override HttpRequestMessage CreateRequest(
            HttpMethod methodType,
            string uri,
            object[] arguments = null,
            (string scheme, string token) authData = default)
        {
            var url = Constants.FORGE_URL + uri;
            return base.CreateRequest(methodType, url, arguments, authData);
        }

        private HttpRequestMessage CreateHttpRequestMessage(ForgeSettings settings, string command, object[] arguments)
        {
            var request = CreateRequest(
                settings.MethodType,
                command,
                arguments,
                settings.IsAuthorized
                    ? (Constants.AUTHORIZATION_SCHEME, Token: settings.UseAppToken ? GetAppToken() : GetToken())
                    : default);

            if (settings.CreateContent != null)
            {
                request.Content = settings.CreateContent();
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
                request.Content = new StringContent(
                    serializedData,
                    Encoding.Default,
                    MEDIA_TYPE_JSON);

                request.Content.Headers.ContentType = new MediaTypeHeaderValue(CONTENT_TYPE);
            }

            return request;
        }
    }
}
