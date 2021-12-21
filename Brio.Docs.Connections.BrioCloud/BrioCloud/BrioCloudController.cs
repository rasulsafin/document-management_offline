using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.External;
using Brio.Docs.External.Utils;
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
                return $"/remote.php/dav/files/{username}";
            }
        }

        public async Task<IEnumerable<CloudElement>> GetListAsync(string path = "/")
        {
            var result = new List<CloudElement>();

            if (!path.Contains(RootPath))
            {
                path = RootPath + path;
            }

            var response = await client.Propfind(path);

            if (response.StatusCode == 401)
            {
                throw new UnauthorizedAccessException(response.Description);
            }
            else if (!response.IsSuccessful)
            {
                throw new FileNotFoundException(response.Description);
            }

            var items = BrioCloudElement.GetElements(response.Resources, path);
            result.AddRange(items);

            return result;
        }

        public async Task<bool> DownloadFileAsync(string href, string saveFilePath)
        {
            if (!href.Contains(RootPath))
            {
                href = RootPath + href;
            }

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

                using (var writer = File.OpenWrite(saveFilePath))
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

        public async Task<string> UploadFileAsync(string directoryHref, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            string cloudName = PathManager.FileName(directoryHref, fileInfo.Name);

            using (var reader = fileInfo.OpenRead())
            {
                var response = await client.PutFile(RootPath + cloudName, reader);

                if (response.IsSuccessful)
                {
                    return cloudName;
                }
            }

            return null;
        }

        public async Task<bool> DeleteAsync(string href)
        {
            if (!href.Contains(RootPath))
            {
                href = RootPath + href;
            }

            var response = await client.Delete(href);

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
            if (!href.Contains(RootPath))
            {
                href = RootPath + href;
            }

            var result = await client.Propfind(href);

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

                var sb = new StringBuilder();
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
            if (!path.Contains(RootPath))
            {
                path = RootPath + path;
            }

            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var response = await client.PutFile(path, reader);

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

        public async Task<CloudElement> CreateDirAsync(string path, string nameDir)
        {
            string newPath = PathManager.DirectoryName(path, nameDir);
            var response = await client.Mkcol(RootPath + newPath);

            if (response.IsSuccessful)
            {
                return new BrioCloudElement();
            }

            return null;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
