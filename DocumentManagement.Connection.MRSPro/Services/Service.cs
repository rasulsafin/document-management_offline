using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class Service
    {
        public Service(MrsProHttpConnection connection)
        {
            HttpConnection = connection;
        }

        protected MrsProHttpConnection HttpConnection { get; }

        protected string GetValueString(IReadOnlyCollection<string> collection)
        {
            StringBuilder str = new StringBuilder();
            var count = collection.Count - 1;

            for (int i = 0; i < count; i++)
                str.Append(collection.ElementAt(i)).Append(',');

            str.Append(collection.ElementAt(count));

            return str.ToString();
        }

        protected async Task<T> GetById<T>(string ids, string methodName)
        {
            return await HttpConnection.SendAsync<T>(
                HttpMethod.Get,
                methodName,
                arguments: new object[] { ids });
        }
    }
}
