﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.LementPro
{
    public class NetConnector : IDisposable
    {
        private static readonly double TIMEOUT = 10;

        private readonly HttpClient client;

        public NetConnector()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(TIMEOUT) };

        public void Dispose()
            => client.Dispose();

        public HttpRequestMessage CreateRequest(
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
            using var response = await SendRequestAsync(request, authData);
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(content);
            return jObject;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
                HttpRequestMessage request,
                (string scheme, string token) authData = default,
                HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            if (authData != default)
                request.Headers.Authorization = new AuthenticationHeaderValue(authData.scheme, authData.token);
            var response = await client.SendAsync(request, completionOption);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
