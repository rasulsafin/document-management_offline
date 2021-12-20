﻿using System;
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
        private List<string> tables = new List<string>();
        private List<string> directories = new List<string>();

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
            var foldersInPath = directoryName.Split('/');
            var folderToCheck = new StringBuilder();
            foreach (var pathPart in foldersInPath)
            {
                folderToCheck.Append('/').Append(pathPart);
                var createResult = await CreateDirectoryIfNecessary(folderToCheck.ToString().Trim('/'));
                if (!createResult)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CheckTableDir<T>()
        {
            string tableName = typeof(T).Name;
            bool result = tables.Any(x => x == tableName);
            if (result)
                return true;

            IEnumerable<CloudElement> list = await controller.GetListAsync(PathManager.GetTablesDir());
            foreach (CloudElement element in list)
            {
                if (element.IsDirectory)
                {
                    tables.Add(element.DisplayName);
                }

                if (element.DisplayName == tableName)
                {
                    result = true;
                }
            }

            if (!result)
            {
                await controller.CreateDirAsync(PathManager.GetTablesDir(), tableName);
            }

            return result;
        }

        private async Task<bool> CreateDirectoryIfNecessary(string directoryName)
        {
            bool directoryExists = directories.Any(x => x == directoryName);
            if (directoryExists)
            {
                return true;
            }

            var directory = PathManager.GetNestedDirectory(directoryName);
            directoryName = directory.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            var list = await controller.GetListAsync(directory);

            foreach (CloudElement element in list)
            {
                if (element.IsDirectory)
                {
                    directories.Add(element.DisplayName);
                }

                if (element.DisplayName == directoryName)
                {
                    directoryExists = true;
                }
            }

            if (!directoryExists)
            {
                var createdFolder = await controller.CreateDirAsync(PathManager.GetRootDirectory(), directoryName);
                return createdFolder != null;
            }

            return true;
        }
    }
}
