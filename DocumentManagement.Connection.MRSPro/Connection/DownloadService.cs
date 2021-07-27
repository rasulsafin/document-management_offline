using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class DownloadService : Service
    {
        private static readonly string BASE_URL = "/download";

        public DownloadService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        internal async Task<bool> Download(string id,
            string parentId,
            bool tokenOnly,
            string type)
        {
            string query = $"?ids={id}&parentId={parentId}&tokenOnly={tokenOnly}&type={type}";

            try
            {
                var uri = await HttpConnection.GetUri(BASE_URL + query);
                string dirPath = "Downloads\\";
                string path = dirPath + uri.Segments[uri.Segments.Length-1];

                Directory.CreateDirectory(dirPath);

                using (WebClient webClient = new WebDownload())
                {
                    await webClient.DownloadFileTaskAsync(uri, path);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class WebDownload : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            if (request != null)
            {
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return request;
        }
    }
}
