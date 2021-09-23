using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Connections.LementPro.Properties;
using Brio.Docs.External;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Brio.Docs.Connections.LementPro.LementProConstants;

namespace Brio.Docs.Connections.LementPro.Utilities
{
    /// <summary>
    /// Work with HttpConnection common for different IConnections.
    /// Handles work with http requests and responses to local services work with json only.
    /// </summary>
    public class HttpRequestUtility : IDisposable
    {
        private readonly ILogger<HttpRequestUtility> logger;
        private readonly JsonSerializerSettings jsonSerializerSettings =
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private HttpConnection connector;

        public HttpRequestUtility(ILogger<HttpRequestUtility> logger)
        {
            this.logger = logger;
            this.connector = new HttpConnection();
            logger.LogTrace("HttpRequestUtility created");
        }

        public string Token { get; set; }

        public Func<Task> EnsureAccessValidAsync { get; set; }

        public void Dispose()
        {
            connector.Dispose();
            GC.SuppressFinalize(this);
            logger.LogTrace("HttpRequestUtility disposed");
        }

        /// <summary>
        /// Connects to LementPro system and returns auth data.
        /// </summary>
        /// <param name="login">Login to connect.</param>
        /// <param name="password">Users password.</param>
        /// <returns>Tuple containing token and date when it expires.</returns>
        protected internal async Task<(string token, string expires)> Connect(string login, string password)
        {
            logger.LogTrace("Connect started with login: {@Login}", login);
            var authData = new AuthorizationData
            {
                LoginName = login,
                Password = password,
                RememberMe = true,
            };

            var response = await GetHttpResponseAsync(Resources.MethodAuthenticationLogin, data: authData);
            var token = ParseCookieFromResponse(response, RESPONSE_COOKIES_AUTH_NAME);
            var expires = ParseCookieFromResponse(response, RESPONSE_COOKIES_EXPIRES_NAME);
            logger.LogDebug("Token expires: {@Expires}", expires);

            return (token, expires);
        }

        protected internal async Task<JToken> GetResponseAsync<TData>(string url, TData data = default, HttpMethod requestType = null)
        {
            logger.LogTrace(
                "GetResponseAsync [{@HttpMethod}] started with data: {@Data}\r{@Url}\r",
                requestType,
                data,
                url);
            await EnsureAccessValidAsync();
            var response = await GetHttpResponseAsync(url, data, requestType);
            var content = await response.Content.ReadAsStringAsync();
            logger.LogDebug("Received response: {@Response}", content);
            return JToken.Parse(content);
        }

        protected internal async Task<JToken> GetResponseWithoutDataAsync(string url, HttpMethod requestType = null)
        {
            logger.LogTrace("GetResponseWithoutDataAsync [{@HttpMethod}] started\r{@Url}\r", requestType, url);
            await EnsureAccessValidAsync();
            var response = await GetHttpResponseAsync(url, data: (object)null, requestType);
            var content = await response.Content.ReadAsStringAsync();
            return JToken.Parse(content);
        }

        protected internal async Task<Stream> GetResponseStreamAsync<TData>(
            string url,
            TData data = default)
        {
            logger.LogTrace(
                "GetResponseStreamAsync started with data: {@Data}\r{@Url}\r",
                data,
                url);
            var response = await GetHttpResponseAsync(url, data, completionOption: HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync();
        }

        protected internal async Task<JToken> SendStreamAsync(string url, Stream stream, string fileName, string boundary, HttpMethod requestType = null)
        {
            logger.LogTrace(
                "SendStreamAsync [{@HttpMethod}] started with fileName: {Name}, boundary: {Boundary}\r{@Url}\r",
                requestType,
                fileName,
                boundary,
                url);
            var response =
                await connector.SendRequestAsync(() => CreateRequest(url, stream, fileName, boundary, requestType));
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogDebug("Received response: {@Response}", responseContent);
            return JToken.Parse(responseContent);
        }

        protected internal async Task<JToken> SendStreamWithDataAsync(
            string url,
            Stream stream,
            string fileName,
            string boundary,
            Dictionary<string, string> data = default,
            HttpMethod requestType = null)
        {
            logger.LogTrace(
                "SendStreamWithDataAsync [{@HttpMethod}] started with fileName: {Name}, boundary: {Boundary}, data:{@Data}\r{@Url}\r",
                requestType,
                fileName,
                boundary,
                data,
                url);
            var response = await connector.SendRequestAsync(
                () => CreateRequest(url, stream, fileName, boundary, data, requestType));
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogDebug("Received response: {@Response}", responseContent);
            return JToken.Parse(responseContent);
        }

        protected async Task<HttpResponseMessage> GetHttpResponseAsync<TData>(string url,
            TData data = default,
            HttpMethod requestType = null,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            logger.LogTrace(
                "GetHttpResponseAsync [{@HttpMethod}] started with data:{@Data}\r{@Url}\r",
                requestType,
                data,
                url);

            var response = await connector.SendRequestAsync(
                () => CreateRequest(url, data, requestType),
                completionOption: completionOption);
            return response;
        }

        protected string ParseCookieFromResponse(HttpResponseMessage response, string cookieName)
        {
            logger.LogTrace("ParseCookieFromResponse started");
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookie))
            {
                var cookies = setCookie.FirstOrDefault()?.Split(RESPONSE_COOKIE_VALUES_SEPARATOR);
                if (cookies == null)
                    return default;

                foreach (var cookie in cookies)
                {
                    var parsedValue = cookie.Trim().Split(RESPONSE_COOKIE_KEY_VALUE_SEPARATOR);
                    if (parsedValue.Length == 2 && parsedValue[0] == cookieName)
                    {
                        return parsedValue[1];
                    }
                }
            }

            return default;
        }

        protected HttpRequestMessage InitializeRequest(string url, HttpMethod requestType = null)
        {
            logger.LogTrace("InitializeRequest [{@HttpMethod}] started\r{@Url}\r", requestType, url);
            requestType ??= HttpMethod.Post;
            var fullUrl = $"{Resources.UrlServer}{url}";

            var request = connector.CreateRequest(
                requestType,
                fullUrl,
                authData: (STANDARD_AUTHENTICATION_SCHEME, Token));

            return request;
        }

        private HttpRequestMessage CreateRequest(
            string url,
            Stream stream,
            string fileName,
            string boundary,
            HttpMethod requestType)
        {
            var request = InitializeRequest(url, requestType);
            var content = new MultipartFormDataContent { { new StreamContent(stream), boundary, fileName } };
            request.Content = content;
            return request;
        }

        private HttpRequestMessage CreateRequest(
            string url,
            Stream stream,
            string fileName,
            string boundary,
            Dictionary<string, string> data,
            HttpMethod requestType)
        {
            var request = InitializeRequest(url, requestType);
            var content = new MultipartFormDataContent { { new StreamContent(stream), boundary, fileName } };

            if (data != default)
            {
                foreach (var item in data)
                    content.Add(new StringContent(item.Value), item.Key);
            }

            request.Content = content;
            return request;
        }

        private HttpRequestMessage CreateRequest<TData>(string url, TData data, HttpMethod requestType)
        {
            var request = InitializeRequest(url, requestType);

            if (data != null)
            {
                var serializedData = JsonConvert.SerializeObject(data, jsonSerializerSettings);
                request.Content = new StringContent(serializedData, Encoding.UTF8, STANDARD_CONTENT_TYPE);
            }

            request.Headers.Add(CONTENT_ACCEPT_LANGUAGE, STANDARD_ACCEPT_LANGUAGE);
            return request;
        }
    }
}
