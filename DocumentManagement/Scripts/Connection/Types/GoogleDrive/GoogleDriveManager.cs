using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using MRS.Bim.DocumentManagement.Utilities;
using File = Google.Apis.Drive.v3.Data.File;

namespace MRS.Bim.DocumentManagement.GoogleDrive
{
    public class GoogleDriveManager : ICloudManager
    {
        private static readonly string[] SCOPES = {DriveService.Scope.Drive};
        private static readonly string APPLICATION_NAME = "BRIO MRS";
        private static readonly string APP_PROPERTIES_RESOURCE = "GoogleApp";
        
        private readonly AppProperty appProperty;
        private DriveService service;
        private UserCredential credential;
        private CancellationTokenSource cancellationTokenSource;

        public GoogleDriveManager()
            => appProperty = AppProperty.LoadFromResources(APP_PROPERTIES_RESOURCE);

        public async Task<bool> Connect()
        {
            cancellationTokenSource = new CancellationTokenSource();

            var credPath =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "token.json") ;
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = appProperty.clientId,
                        ClientSecret = appProperty.clientSecret
                    },
                    SCOPES,
                    "user",
                    cancellationTokenSource.Token,
                    new FileDataStore(credPath, true));
            
            service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = APPLICATION_NAME,
            });
            return true;
        }

        public void Disconnect() => credential?.RevokeTokenAsync(CancellationToken.None);

        public async Task<bool> Download(CloudItem item, string path)
        {
            using (var stream = System.IO.File.Create(path))
            {
                var request = service.Files.Get(item.ID);
                await request.DownloadAsync(stream);
                return true;
            }
        }

        public async Task<bool> Upload(string path, string parentID, CloudItem item)
        {
            var onCloud = await GetItems(parentID, item.Name);
            var file = new File
            {
                Name = item.Name
            };

            var contentType = (string) null;
            IUploadProgress result;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (onCloud.Any())
                {
                    var request = service.Files.Update(file, onCloud.First().ID, stream, contentType);
                    result = await request.UploadAsync();
                }
                else
                {
                    file.Parents = new List<string> {parentID};
                    var request = service.Files.Create(file, stream, contentType);
                    result = await request.UploadAsync();
                }
            }

            return result.Exception == null;
        }

        public async Task<List<CloudItem>> GetItems(string parentId = "", string partOfName = "", bool? folders = null)
        {
            var result = new List<File>();
            var nextPageToken = "";
            do
            {
                var request = service.Files.List();
                request.Fields = "nextPageToken, files(id, name, size, mimeType, modifiedTime)";
                request.PageSize = 100;
                var q = new List<string>();
                if (!string.IsNullOrEmpty(partOfName))
                    q.Add($"name contains '{partOfName}'");
                if (!string.IsNullOrEmpty(parentId))
                    q.Add($"'{parentId}' in parents");
                if (folders.HasValue)
                    q.Add((folders.Value ? "" : "not ") + "mimeType contains 'folder'");
                q.Add("trashed = false");
                request.Q = q.Aggregate((a, b) => $"{a} and {b}");
                if (!string.IsNullOrEmpty(nextPageToken))
                    request.PageToken = nextPageToken;
                var fileList = await request.ExecuteAsync();
                result.AddRange(fileList.Files);
                nextPageToken = fileList.NextPageToken;
            } while (!string.IsNullOrEmpty(nextPageToken));

            return result.Select(x => new CloudItem
            {
                ID = x.Id,
                IsFolder = x.MimeType.Contains("folder"),
                ModifiedTime = x.ModifiedTime,
                Name = x.Name,
                Size = x.Size
            }).ToList();
        }

        public async Task<CloudItem> CreateAppDirectory(string appPath)
        {
            var file = new File
            {
                Name = appPath,
                MimeType = "application/vnd.google-apps.folder"
            };
            var request = service.Files.Create(file);
            var result = await request.ExecuteAsync();
            return new CloudItem
            {
                ID = result.Id,
                IsFolder = true,
                Name = result.Name
            };
        }

        public void Cancel()
            => cancellationTokenSource?.Cancel();
    }
}