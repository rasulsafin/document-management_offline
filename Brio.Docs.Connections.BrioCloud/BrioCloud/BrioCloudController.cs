using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.External;
using Brio.Docs.External.Utils;
using Brio.Docs.Integration.Dtos;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudController : IDisposable
    {
        private const string BASE_URI = "https://cloud.briogroup.ru";

        private IWebDavClient client;
        private string username;

        public BrioCloudController(string username, string password)
        {
            this.username = username;

            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BASE_URI),
            };
            httpClient.DefaultRequestHeaders.Add("OCS-APIRequest", "true");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);

            client = new WebDavClient(httpClient);

            GetListAsync().Wait();
        }

        private string RootPath
        {
            get
            {
                return $"remote.php/dav/files/{username}";
            }
        }

        public async Task<IEnumerable<CloudElement>> GetListAsync(string path = "/")
        {
            var response = await client.Propfind(RootPath + path);

            if (!response.IsSuccessful)
            {
                throw new FileNotFoundException(response.Description);
            }

            List<BrioCloudElement> items = BrioCloudElement.GetElements(response.Resources);
            return items;
        }

        public async Task<bool> DownloadFileAsync(string href, string fileName)
        {
            var result = await client.Propfind(href);

            if (!result.IsSuccessful)
            {
                throw new FileNotFoundException();
            }

            using (var response = await client.GetRawFile(href))
            {
                if (!response.IsSuccessful)
                {
                    throw new WebException(response.Description);
                }

                using (var writer = File.OpenWrite(fileName))
                {
                    using (var reader = response.Stream)
                    {
                        const int BUFFER_LENGTH = 4096;
                        var buffer = new byte[BUFFER_LENGTH];
                        var count = reader.Read(buffer, 0, BUFFER_LENGTH);
                        while (count > 0)
                        {
                            writer.Write(buffer, 0, count);
                            count = reader.Read(buffer, 0, BUFFER_LENGTH);
                        }
                    }
                }

                return true;
            }
        }

        public async Task<string> UploadFileAsync(string href, string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            string cloudName = PathManager.FileName(href, fileInfo.Name);

            using (var reader = fileInfo.OpenRead())
            {
                var response = await client.PutFile($"{RootPath}{cloudName}", reader);

                if (response.IsSuccessful)
                {
                    return cloudName;
                }
            }

            return null;
        }

        public async Task<bool> DeleteAsync(string href)
        {
            var response = await client.Delete(RootPath + href);

            if (response.IsSuccessful)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> GetContentAsync(string href)
        {
            var result = await client.Propfind(RootPath + href);

            if (!result.IsSuccessful)
            {
                throw new FileNotFoundException();
            }

            using (var response = await client.GetRawFile(RootPath + href))
            {
                if (!response.IsSuccessful)
                {
                    throw new WebException(response.Description);
                }

                StringBuilder sb = new StringBuilder();
                using (var reader = response.Stream)
                {
                    const int BUFFER_LENGTH = 4096;
                    var buffer = new byte[BUFFER_LENGTH];
                    var count = reader.Read(buffer, 0, BUFFER_LENGTH);
                    while (count > 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, count));
                        count = reader.Read(buffer, 0, BUFFER_LENGTH);
                    }
                }

                return sb.ToString();
            }
        }

        public async Task<bool> SetContentAsync(string path, string content)
        {
            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var response = await client.PutFile($"{RootPath}{path}", reader);

                if (response.IsSuccessful)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
