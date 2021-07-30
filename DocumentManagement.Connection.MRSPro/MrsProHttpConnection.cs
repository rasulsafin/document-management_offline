using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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

        internal async Task<Uri> GetUri(string method)
        {
            var response = await GetUriAsync(() => CreateRequest(HttpMethod.Get, method, null, new object[] { }));
            return response;
        }

        internal async Task<IEnumerable<TOut>> GetListOf<TOut>(string method, params object[] args)
            => await SendAsync<IEnumerable<TOut>>(HttpMethod.Get, method, arguments: args);

        internal async Task<TData> PostJson<TData>(string method, TData contentObject)
            => await PostJson<TData, TData>(method, contentObject);

        internal async Task<TData> PostJson<TData>(string method)
            => await PostJson<TData, TData>(method);

        internal async Task<TOut> PostJson<TOut, TData>(string method, TData contentObject)
            => await SendJson<TOut, TData>(HttpMethod.Post, method, contentObject);

        internal async Task<TOut> PostJson<TOut, TData>(string method)
            => await SendJson<TOut, TData>(HttpMethod.Post, method);

        internal async Task<TData> PostJson<TData>(string method, TData contentObject, byte[] file, string filename, string folderId)
            => await SendJson<TData, TData>(HttpMethod.Post, method, contentObject, file, filename, folderId);

        internal async Task<TOut> PatchJson<TOut, TData>(string method, TData contentObject)
            => await SendJson<TOut, TData>(HttpMethod.Patch, method, contentObject);

        internal async Task<TOut> PutJson<TOut, TData>(string method, TData contentObject)
            => await SendJson<TOut, TData>(HttpMethod.Put, method, contentObject);

        internal async Task<TData> PutJson<TData>(string method, TData contentObject, byte[] file, string filename)
            => await SendJson<TData, TData>(HttpMethod.Put, method, contentObject, file, filename);

        internal async Task DeleteJson<TData>(string method, TData contentObject)
            => await SendJson<TData, TData>(HttpMethod.Delete, method, contentObject);

        private async Task<TOut> SendJson<TOut, TData>(HttpMethod httpMethod, string method, TData contentObject)
        {
            var json = JsonConvert.SerializeObject(contentObject);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await SendAsync<TOut>(httpMethod, method, content);
        }

        private async Task<TOut> SendJson<TOut, TData>(HttpMethod httpMethod, string method)
        {
            return await SendAsync<TOut>(httpMethod, method);
        }

        private async Task<TOut> SendJson<TOut, TData>(HttpMethod httpMethod, string method, TData contentObject, byte[] file, string filename, string folderId = null)
        {
            var json = JsonConvert.SerializeObject(contentObject);
            var multipart = new MultipartFormDataContent();

            multipart.Headers.ContentType.MediaType = "multipart/form-data";
            multipart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            multipart.Add(new StringContent(json, Encoding.UTF8, "application/json"));

            var byteContent = new ByteArrayContent(file);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            multipart.Add(byteContent, "file", filename);

            if (folderId != null)
                multipart.Add(new StringContent(folderId, Encoding.UTF8, "text/plain"), "folderId");

            return await SendAsync<TOut>(httpMethod, method, multipart);
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
