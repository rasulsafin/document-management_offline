﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Utilities
{
    public class HttpRequestUtility : IDisposable
    {
        private NetConnector connector;

        public HttpRequestUtility(NetConnector connector)
            => this.connector = connector;

        public void Dispose()
            => connector.Dispose();

        protected internal async Task<(string token, string expires)> Connect(string login, string password)
        {
            var authData = new AuthorizationData
            {
                LoginName = login,
                Password = password,
                RememberMe = true,
            };

            var response = await GetHttpResponseAsync(Resources.AuthenticationLoginMethod, data: authData);
            var token = ParseCookieFromResponse(response, RESPONSE_COOKIES_AUTH_NAME);
            var expires = ParseCookieFromResponse(response, RESPONSE_COOKIES_EXPIRES_NAME);

            return (token, expires);
        }

        protected internal async Task<JToken> GetResponseAsync<TData>(string url, string token = null, TData data = default, HttpMethod requestType = null)
        {
            var response = await GetHttpResponseAsync(url, token, data, requestType);
            var content = await response.Content.ReadAsStringAsync();
            return JToken.Parse(content);
        }

        protected async Task<HttpResponseMessage> GetHttpResponseAsync<TData>(string url, string token = null, TData data = default, HttpMethod requestType = null)
        {
            requestType ??= HttpMethod.Post;
            var fullUrl = $"{Resources.UrlServer}{url}";

            var request = connector.CreateRequest(requestType, fullUrl, authData: (STANDARD_AUTHENTICATION_SCHEME, token));

            if (data != null)
            {
                var serializedData = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(serializedData, Encoding.UTF8, STANDARD_CONTENT_TYPE);
            }

            request.Headers.Add(CONTENT_ACCEPT_LANGUAGE, STANDARD_ACCEPT_LANGUAGE);

            var response = await connector.SendRequestAsync(request);
            return response;
        }

        protected string ParseCookieFromResponse(HttpResponseMessage response, string cookieName)
        {
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
    }
}
