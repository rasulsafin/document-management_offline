using System;
using System.Collections.Generic;
using System.Linq;
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

        private string BaseUrl { get => string.Format(BASE_URL, Auth.CompanyCode); }

        public async Task<TOut> SendAsyncJson<TOut, TData>(HttpMethod methodType, string method, TData contentObject)
        {
            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, "application/json");
            return await SendAsync<TOut>(methodType, method, content);
        }

        public async Task<T> SendAsync<T>(HttpMethod methodType, string method, HttpContent content = null,  object[] arguments = null)
        {
            var url = BaseUrl + method;
            var request = CreateRequest(methodType, url, arguments);
            request.Content = content;
            request.Headers.Add(SCHEME, Auth.Token);

            logger.LogInformation("Request {0} send to {1}", methodType, url);

            var response = await GetResponseAsync(request);
            return response.ToObject<T>();
        }
    }
}
