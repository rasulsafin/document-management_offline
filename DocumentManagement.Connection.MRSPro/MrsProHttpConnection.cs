using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using Newtonsoft.Json;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProHttpConnection : HttpConnection
    {
        private static readonly string SCHEME = "x-auth-token";

        internal static string CompanyCode { get; set; }

        internal static string Token { get; set; }

        private static string BaseUrl { get => string.Format(BASE_URL, CompanyCode); }

        public async Task<TOut> SendAsyncJson<TOut, TData>(HttpMethod methodType, string method, TData contentObject)
        {
            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, "application/json");
            return await SendAsync<TOut>(methodType, method, content);
        }

        public async Task<T> SendAsync<T>(HttpMethod methodType, string method, HttpContent content = null,  object[] arguments = null)
        {
            var url = BaseUrl + method;
            var request = CreateRequest(methodType, url, new object[] { arguments }, (SCHEME, Token));
            request.Content = content;
            var response = await GetResponseAsync(request);
            return response.ToObject<T>();
        }
    }
}
