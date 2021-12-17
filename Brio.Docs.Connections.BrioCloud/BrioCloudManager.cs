using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.External;
using Brio.Docs.External.Utils;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudManager : ICloudManager
    {
        private BrioCloudController controller;

        public BrioCloudManager(BrioCloudController controller)
        {
            this.controller = controller;
        }

        public string RootDirectoryHref { get; private set; }

        public async Task<IEnumerable<CloudElement>> GetRemoteDirectoryFiles(string directoryPath = "/")
        {
            try
            {
                return await controller.GetListAsync(directoryPath);
            }
            catch (FileNotFoundException)
            {
                return Enumerable.Empty<CloudElement>();
            }
        }

        public async Task<bool> PullFile(string href, string fileName)
        {
            try
            {
                return await controller.DownloadFileAsync(href, fileName);
            }
            catch (FileNotFoundException)
            {
            }

            return false;
        }

        public async Task<string> PushFile(string remoteDirName, string fullPath)
        {
            try
            {
                await CheckDirectory(remoteDirName);
                string path = PathManager.GetNestedDirectory(remoteDirName);
                var created = await controller.UploadFileAsync(path, fullPath);

                return created;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteFile(string href)
        {
            return await controller.DeleteAsync(href);
        }

        public async Task<T> Pull<T>(string id)
        {
            try
            {
                if (await CheckTableDir<T>())
                {
                    string tableName = typeof(T).Name;
                    string path = PathManager.GetRecordFile(tableName, id);
                    string json = await controller.GetContentAsync(path);
                    T @object = JsonConvert.DeserializeObject<T>(json);
                    return @object;
                }
            }
            catch (FileNotFoundException)
            {
            }

            return default;
        }

        public async Task<bool> Push<T>(T @object, string id)
        {
            try
            {
                await CheckTableDir<T>();
                string tableName = typeof(T).Name;
                string path = PathManager.GetRecordFile(tableName, id);
                string json = JsonConvert.SerializeObject(@object);
                return await controller.SetContentAsync(path, json);
            }
            catch (Exception)
            {
            }

            return false;
        }

        public async Task<bool> Delete<T>(string id)
        {
            if (await CheckTableDir<T>())
            {
                string tableName = typeof(T).Name;
                string path = PathManager.GetRecordFile(tableName, id);
                return await controller.DeleteAsync(path);
            }

            return false;
        }

        public async Task<List<T>> PullAll<T>(string path)
        {
            var resultCollection = new List<T>();
            try
            {
                var elements = await GetRemoteDirectoryFiles(path);
                foreach (var item in elements.Where(e => !e.IsDirectory))
                {
                    var remoteItem = await Pull<T>(Path.GetFileNameWithoutExtension(item.Href));
                    resultCollection.Add(remoteItem);
                }
            }
            catch (FileNotFoundException)
            {
            }

            return resultCollection;
        }

        public async Task<ConnectionStatusDto> GetStatusAsync()
        {
            try
            {
                var list = await controller.GetListAsync(RootDirectoryHref);
                if (list != null)
                {
                    return new ConnectionStatusDto()
                    {
                        Status = RemoteConnectionStatus.OK,
                        Message = "Good",
                    };
                }
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = ex.Message,
                };
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.NeedReconnect,
                Message = "Not connect",
            };
        }

        private async Task<bool> CheckDirectory(string directoryName)
        {
            return true;
        }

        private async Task<bool> CheckTableDir<T>()
        {
            return true;
        }
    }
}
