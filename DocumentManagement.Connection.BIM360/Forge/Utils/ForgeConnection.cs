using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using MRS.DocumentManagement.Connection.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge
{
    public class ForgeConnection : HttpConnection, IDisposable
    {
        private const string MEDIA_TYPE_JSON = "text/json";
        private const string CONTENT_TYPE = "application/vnd.api+json";

        public ForgeConnection()
            : base()
            => Timeout = 30;

        public string Token { get; set; }

        public static string SetFilters(string uri, IEnumerable<(string filteringField, string filteringValue)> filters = null)
        {
            var stringBuilder = new StringBuilder(uri);
            if (filters != null)
            {
                if (stringBuilder[^1] != '&')
                    stringBuilder.Append('&');
                foreach ((string field, string filterValue) in filters)
                    stringBuilder.AppendFormat(Constants.FILTER_QUERY_PARAMETER, field, filterValue);
            }

            if (stringBuilder.Length > 0 && stringBuilder[^1] != '&')
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }

        public async Task<Stream> GetResponseStreamAuthorizedAsync(
                HttpMethod methodType,
                string command,
                params object[] arguments)
            => await GetResponseStreamAuthorizedAsync(methodType, command, authData: (Constants.AUTHORIZATION_SCHEME, Token), arguments);

        public async Task<JObject> SendAsync(
                ForgeSettings settings,
                string command,
                params object[] arguments)
        {
            using var request = CreateRequest(
                settings.MethodType,
                command,
                arguments,
                settings.IsAuthorized ? (Constants.AUTHORIZATION_SCHEME, Token) : default);

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

        public override HttpRequestMessage CreateRequest(HttpMethod methodType, string uri, object[] arguments = null, (string scheme, string token) authData = default)
        {
            var url = Resources.ForgeUrl + uri;
            return base.CreateRequest(methodType, url, arguments, authData);
        }
    }
}
