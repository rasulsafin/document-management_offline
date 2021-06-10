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

        private static string BaseUrl => string.Format(BASE_URL, Auth.CompanyCode);

        public async Task<IEnumerable<TOut>> GetAll<TOut>(string method)
        {
            return await SendAsync<IEnumerable<TOut>>(HttpMethod.Get, method);
        }

        public async Task<IEnumerable<TOut>> GetByIds<TOut>(string method, IReadOnlyCollection<string> ids)
        {
            StringBuilder str = new ();
            var count = ids.Count - 1;

            for (int i = 0; i < count; i++)
                str.Append(ids.ElementAt(i)).Append(',');

            str.Append(ids.ElementAt(count));

            return await SendAsync<IEnumerable<TOut>>(HttpMethod.Get, method, arguments: new object[] { str.ToString() });
        }

        public async Task<IEnumerable<TOut>> GetByIds<TOut>(string method, string id)
        {
            return await SendAsync<IEnumerable<TOut>>(HttpMethod.Get, method, arguments: new object[] { id });
        }

        public async Task<TOut> SendAsyncJson<TOut, TData>(string method, TData contentObject)
        {
            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, "application/json");
            return await SendAsync<TOut>(HttpMethod.Post, method, content);
        }

        private async Task<T> SendAsync<T>(HttpMethod methodType, string method, HttpContent content = null,  params object[] arguments)
        {
            var url = BaseUrl + method;
            var request = CreateRequest(methodType, url, arguments);
            request.Content = content;
            request.Headers.Add(SCHEME, Auth.Token);

            var response = await GetResponseAsync(request);
            return response.ToObject<T>();
        }
    }
}
