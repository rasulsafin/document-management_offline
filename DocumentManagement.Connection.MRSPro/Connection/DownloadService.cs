using MRS.DocumentManagement.Connection.MrsPro.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;


namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class DownloadService : Service
    {
        private static readonly string BASE_URL = "/download";
        private static readonly string BASE_URL_ATTACHMENT = "/attachment";


        public DownloadService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        internal async Task<bool> Download(string id,
            string parentId)
        {
            Uri uri = null;

            try
            {
                uri = await GetUriAttachment(id);
            }
            catch
            {
                uri = await GetUriPlan(id, parentId);
            }

            string dirPath = "Downloads\\";

            string name = WebUtility.UrlDecode(uri.Segments[uri.Segments.Length - 1]);
            string path = dirPath + name;

            Directory.CreateDirectory(dirPath);

            using (WebClient webClient = new WebDownload())
            {
                await webClient.DownloadFileTaskAsync(uri, path);
            }

            return true;
        }

        internal async Task<Uri> GetUriPlan(string id,
            string parentId)
        {
            string query = $"?ids={id}&parentId={parentId}&tokenOnly=false&type=plan";
            var uri = await HttpConnection.GetUri(BASE_URL + query);

            return uri;
        }

        internal async Task<Uri> GetUriAttachment(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL_ATTACHMENT);
            var attachment = listOfAllObjectives.Where(o => o.Id == id).ToArray()[0];

            string link = "https://s3-eu-west-1.amazonaws.com/plotpad-org/" + attachment.UrlToFile;

            var uri = new Uri(link);

            return uri;
        }

        private class WebDownload : WebClient
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
}
