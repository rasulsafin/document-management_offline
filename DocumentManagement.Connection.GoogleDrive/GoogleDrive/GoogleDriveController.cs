using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleDriveController : ICloudController
    {
        // https://developers.google.com/drive/api/v3/search-files?hl=ru

        private static readonly string REQUEST_FIELDS = "nextPageToken, files(id, name, size, mimeType, modifiedTime, createdTime)";
        private static readonly string REQUEST_All_FIELDS = "nextPageToken, files(*)";
        private static readonly string MIME_TYPE_FOLDER = "application/vnd.google-apps.folder";
        private CancellationTokenSource cancellationTokenSource;
        private UserCredential credential;
        private DriveService service;

        public GoogleDriveController()
        {
        }

        public async Task InitializationAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();

            ClientSecrets clientSecrets = new ClientSecrets
            {
                ClientId = GoogleDriveAuth.CLIENT_ID,
                ClientSecret = GoogleDriveAuth.CLIENT_SECRET,
            };
            // TODO: Найти место для этой помойки
            FileDataStore dataStore = new FileDataStore("token.json", true);
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    GoogleDriveAuth.SCOPES,
                    "user",
                    cancellationTokenSource.Token,
                    dataStore
                    );

            service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = GoogleDriveAuth.APPLICATION_NAME,
            });
        }

        #region PROPFIND

        public async Task<IEnumerable<DiskElement>> GetListAsync(string id = "")
        {
            var result = new List<Google.Apis.Drive.v3.Data.File>();
            var nextPageToken = string.Empty;

            do
            {
                var request = service.Files.List();
                request.Fields = REQUEST_FIELDS;
                request.PageSize = 100;
                request.Spaces = "Drive";
                if (!string.IsNullOrWhiteSpace(id))
                    request.Q = $"'{id}' in parents";
                else
                    request.Q = $"'root' in parents";
                if (!string.IsNullOrEmpty(nextPageToken))
                    request.PageToken = nextPageToken;
                var fileList = await request.ExecuteAsync();
                result.AddRange(fileList.Files);
                nextPageToken = fileList.NextPageToken;
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            List<GoogleDriveElement> elements = new List<GoogleDriveElement>();
            foreach (Google.Apis.Drive.v3.Data.File item in result)
            {
                var element = new GoogleDriveElement(item);
                elements.Add(element);
            }

            return elements;
        }

        public async Task<IEnumerable<DiskElement>> GetListAsync(string parentId, string partOfName)
        {
            var result = new List<Google.Apis.Drive.v3.Data.File>();
            var nextPageToken = string.Empty;

            do
            {
                var request = service.Files.List();
                request.Fields = REQUEST_FIELDS;
                request.PageSize = 100;
                request.Spaces = "Drive";

                var q = new List<string>();
                q.Add($"'{parentId}' in parents");
                q.Add($"name contains '{partOfName}'");
                request.Q = q.Aggregate((a, b) => $"{a} and {b}");

                if (!string.IsNullOrEmpty(nextPageToken))
                    request.PageToken = nextPageToken;

                var fileList = await request.ExecuteAsync();

                result.AddRange(fileList.Files);
                nextPageToken = fileList.NextPageToken;
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            List<GoogleDriveElement> elements = new List<GoogleDriveElement>();
            foreach (Google.Apis.Drive.v3.Data.File item in result)
            {
                var element = new GoogleDriveElement(item);
                elements.Add(element);
            }

            return elements;
        }

        public async Task<DiskElement> GetInfoAsync(string id)
        {
            try
            {
                var request = service.Files.Get(id);
                request.Fields = "*";
                var file = await request.ExecuteAsync();
                if (file != null)
                    return new GoogleDriveElement(file);
            }
            catch { }

            return null;
        }

        #endregion
        #region Create Directory
        public async Task<bool> CreateDirAsync(string idParent, string nameDir)
        {

            var fileDrive = new Google.Apis.Drive.v3.Data.File
            {
                Name = nameDir,
                MimeType = MIME_TYPE_FOLDER,
            };
            if (!string.IsNullOrWhiteSpace(idParent))
                fileDrive.Parents = new List<string>() { idParent };

            var request = service.Files.Create(fileDrive);
            var result = await request.ExecuteAsync();
            if (result != null)
                return true;
            return false;

        }
        #endregion
        #region Content
        /// <summary>
        /// Write the content to a file and upload it.
        /// </summary>
        /// <param name="path">The path to the file on the disk. </param>
        /// <param name="content">Content</param>
        /// <param name="progressChenge"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<bool> SetContentAsync(string content, string idParent, string name)
        {
            var info = await GetInfoAsync(idParent);
            IUploadProgress result = null;

            if (info.IsDirectory)
            {
                var contentType = (string)null;
                var fileDrive = new Google.Apis.Drive.v3.Data.File
                {
                    Name = name,
                };

                var infos = await GetListAsync(idParent);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    if (infos.Any(x => x.DisplayName == name))
                    {
                        var href = infos.First().Href;
                        var request = service.Files.Update(fileDrive, href, stream, contentType);
                        result = await request.UploadAsync();
                    }
                    else
                    {
                        fileDrive.Parents = new List<string> { idParent };
                        var request = service.Files.Create(fileDrive, stream, contentType);
                        result = await request.UploadAsync();
                    }
                }
            }

            return result.Exception == null;
        }

        /// <summary>
        /// Downloads the file and returns its contents.
        /// </summary>
        /// <param name="path">The path to the file on the disk. </param>
        /// <param name="updateProgress"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="DirectoryNotFoundException">Directory Not Found Exception</exception>
        /// <exception cref="FileNotFoundException">File Not Found Exception</exception>
        public async Task<string> GetContentAsync(string idParent, string name)
        {
            var info = await GetInfoAsync(idParent);
            IUploadProgress result = null;

            if (info.IsDirectory)
            {
                var infos = await GetListAsync(idParent);
                if (infos.Any(x => x.DisplayName == name))
                {
                    using (var stream = new MemoryStream())
                    {
                        var href = infos.First().Href;
                        var request = service.Files.Get(href);
                        await request.DownloadAsync(stream);
                        var buffer = stream.ToArray();
                        var content = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        return content;
                    }
                }
            }

            return string.Empty;
        }

        #endregion
        #region Download File

        /// <summary>
        /// Downloading a file (GET).
        /// </summary>
        /// <param name="href">The path to the file on the disk. </param>
        /// <param name="currentPath">The path to the file on the computer.</param>
        /// <param name="updateProgress"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/get.html/.</remarks>
        /// <exception cref="DirectoryNotFoundException">Directory Not Found Exception</exception>
        /// <exception cref="FileNotFoundException">File Not Found Exception</exception>
        public async Task<bool> DownloadFileAsync(string href, string currentPath, Action<ulong, ulong> updateProgress = null)
        {
            using (var stream = System.IO.File.Create(currentPath))
            {
                var request = service.Files.Get(href);
                await request.DownloadAsync(stream);
                return true;
            }
        }
        #endregion
        #region Delete file and directory

        /// <summary>
        /// Deleting a file or directory at the specified path.
        /// </summary>
        /// <param name="path">the path to delete the file or gurney.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="FileNotFoundException" >No file needed</exception>
        public async Task<bool> DeleteAsync(string href)
        {
            var request = service.Files.Delete(href);
            var response = await request.ExecuteAsync();
            return true;
        }
        #endregion
        #region Load File

        /// <summary>
        /// Load File
        /// </summary>
        /// <param name="href">The path to the file on the disk. </param>
        /// <param name="fileName">The path to the file on the computer.</param>
        /// <param name="progressChenge"> to transfer the progress </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">The server timeout has expired.</exception>
        public async Task<bool> LoadFileAsync(string idParent, string fileName, Action<ulong, ulong> progressChenge = null)
        {
            var info = await GetInfoAsync(idParent);
            IUploadProgress result = null;

            if (info.IsDirectory)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                var contentType = (string)null;
                var fileDrive = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileInfo.Name,
                };

                var infos = await GetListAsync(idParent);

                using (var stream = fileInfo.OpenRead())
                {
                    if (infos.Any(x => x.DisplayName == fileInfo.Name))
                    {
                        var href = infos.First().Href;
                        var request = service.Files.Update(fileDrive, href, stream, contentType);
                        result = await request.UploadAsync();
                    }
                    else
                    {
                        fileDrive.Parents = new List<string> { idParent };
                        var request = service.Files.Create(fileDrive, stream, contentType);
                        result = await request.UploadAsync();
                    }
                }
            }

            return result.Exception == null;
        }
        #endregion
        #region COPY TODO

        /// <summary>
        /// (COPY).
        /// </summary>
        /// <param name="originPath"> path to the original file </param>
        /// <param name="copyPath"> the way in which you need to copy </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">The server timeout has expired.</exception>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/copy.html.</remarks>
        public Task<bool> CopyAsync(string originPath, string copyPath)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region MOVE TODO

        /// <summary>
        /// (MOVE).
        /// </summary>
        /// <param name="originPath"> path to the original file </param>
        /// <param name="movePath"> the way in which it is necessary to move the file(or rename) </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        /// <exception cref="TimeoutException">The server timeout has expired.</exception>
        /// <remarks>https://yandex.ru/dev/disk/doc/dg/reference/copy.html.</remarks>
        public async Task<bool> MoveAsync(string originPath, string movePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetContentAsync(string path, string content, Action<ulong, ulong> progressChenge = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetContentAsync(string path, Action<ulong, ulong> updateProgress = null)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
