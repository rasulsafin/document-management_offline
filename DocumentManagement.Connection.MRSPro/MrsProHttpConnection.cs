﻿using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.Utils;
using Newtonsoft.Json;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProHttpConnection : HttpConnection
    {
        private static readonly string SCHEME = "x-auth-token";

        private readonly ILogger<MrsProConnection> logger;

        public MrsProHttpConnection(ILogger<MrsProConnection> logger)
        {
            this.logger = logger;
        }

        private static string BaseUrl => string.Format(BASE_URL, Auth.CompanyCode);

        internal async Task<TOut> Get<TOut>(string method)
            => await SendAsync<TOut>(HttpMethod.Get, method);

        internal async Task<IEnumerable<TOut>> GetListOf<TOut>(string method, params object[] args)
            => await SendAsync<IEnumerable<TOut>>(HttpMethod.Get, method, arguments: args);

        internal async Task<TData> PostJson<TData>(string method, TData contentObject)
            => await PostJson<TData, TData>(method, contentObject);

        internal async Task<TOut> PostJson<TOut, TData>(string method, TData contentObject)
            => await SendJson<TOut, TData>(HttpMethod.Post, method, contentObject);

        internal async Task<TOut> PatchJson<TOut, TData>(string method, TData contentObject)
            => await SendJson<TOut, TData>(HttpMethod.Patch, method, contentObject);

        internal async Task DeleteJson<TData>(string method, TData contentObject)
            => await SendJson<TData, TData>(HttpMethod.Delete, method, contentObject);

        private async Task<TOut> SendJson<TOut, TData>(HttpMethod httpMethod, string method, TData contentObject)
        {
            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, "application/json");
            return await SendAsync<TOut>(httpMethod, method, content);
        }

        private async Task<T> SendAsync<T>(HttpMethod methodType, string method, HttpContent content = null,  params object[] arguments)
        {
            var response = await GetResponseAsync(() => CreateRequest(methodType, method, content, arguments));
            return response.ToObject<T>(); // TODO: Fix it
        }

        private HttpRequestMessage CreateRequest(
            HttpMethod methodType,
            string method,
            HttpContent content,
            object[] arguments)
        {
            var url = $"{BaseUrl}{method}";
            var request = CreateRequest(methodType, url, arguments);
            request.Content = content;
            request.Headers.Add(SCHEME, Auth.Token);
            return request;
        }
    }
}