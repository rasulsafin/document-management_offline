using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleDriveManager : ICloudManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        public static readonly string TABLE_DIR = "Tables";
        public static readonly string REC_FILE = "{0}.json";

        private GoogleDriveController controller;
        private bool checkDirApp;
        private Dictionary<string, string> tables = new Dictionary<string, string>();
        private Dictionary<string, string> directories = new Dictionary<string, string>();

        public GoogleDriveManager(GoogleDriveController driveController)
        {
            controller = driveController;
        }

        public string DirAppHref { get; private set; }

        public string DirTableHref { get; private set; }

        public async Task<ConnectionStatusDto> GetStatusAsync()
        {
            try
            {
                var list = await controller.GetListAsync(DirAppHref);
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

        public async Task<bool> Push<T>(T @object, string id)
        {
            try
            {
                var tableHref = await GetTableHref<T>();
                string name = string.Format(REC_FILE, id);
                string json = JsonConvert.SerializeObject(@object);
                return await controller.SetContentAsync(json, tableHref, name);
            }
            catch (Exception)
            {
            }

            return false;
        }

        public async Task<T> Pull<T>(string id)
        {
            try
            {
                var tableHref = await GetTableHref<T>();
                string name = string.Format(REC_FILE, id);
                string json = await controller.GetContentAsync(tableHref, name);
                T @object = JsonConvert.DeserializeObject<T>(json);
                return @object;
            }
            catch (Exception)
            {
            }

            return default;
        }

        public async Task<bool> Delete<T>(string id)
        {
            var tableHref = await GetTableHref<T>();
            string name = string.Format(REC_FILE, id);
            var list = await controller.GetListAsync(tableHref);
            var record = list.FirstOrDefault(x => x.DisplayName == name);
            if (record != null)
            {
                return await controller.DeleteAsync(record.Href);
            }

            return false;
        }

        public async Task<List<T>> PullAll<T>(string path)
        {
            var resultCollection = new List<T>();
            try
            {
                var elements = await GetRemoteDirectoryFiles(path);
                foreach (var item in elements)
                    resultCollection.Add(await Pull<T>(item.Href));
            }
            catch (FileNotFoundException)
            {
            }

            return resultCollection;
        }

        public async Task<string> PushFile(string remoteDirName, string localDirName, string fileName)
        {
            string dirHref = await GetDirHref(remoteDirName);
            var res = await controller.LoadFileAsync(dirHref, Path.Combine(localDirName, fileName));
            return res.Href;
        }

        public async Task<bool> DeleteFile(string href)
        {
            return await controller.DeleteAsync(href);
        }

        public async Task<bool> PullFile(string href, string fileName)
        {
            return await controller.DownloadFileAsync(href, fileName);
        }

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

        private async Task<string> GetDirHref(string dirName)
        {
            await CheckDirApp();
            var res = directories.ContainsKey(dirName);
            if (!res)
            {
                directories.Clear();
                var list = await controller.GetListAsync(DirAppHref);
                foreach (CloudElement element in list)
                {
                    if (element.IsDirectory)
                        directories.Add(element.DisplayName, element.Href);
                    if (element.DisplayName == dirName)
                        res = true;
                }

                if (!res)
                {
                    var dir = await controller.CreateDirAsync(DirAppHref, dirName);
                    directories.Add(dir.DisplayName, dir.Href);
                }
            }

            return directories[dirName];
        }

        private async Task<string> GetTableHref<T>()
        {
            await CheckDirApp();
            string tableName = typeof(T).Name;
            var res = tables.ContainsKey(tableName);
            if (!res)
            {
                tables.Clear();
                var list = await controller.GetListAsync(DirTableHref);
                foreach (CloudElement element in list)
                {
                    if (element.IsDirectory)
                        tables.Add(element.DisplayName, element.Href);
                    if (element.DisplayName == tableName)
                        res = true;
                }

                if (!res)
                {
                    var dir = await controller.CreateDirAsync(DirTableHref, tableName);
                    tables.Add(dir.DisplayName, dir.Href);
                }
            }

            return tables[tableName];
        }

        private async Task CheckDirApp()
        {
            if (!string.IsNullOrWhiteSpace(DirAppHref)) return;
            IEnumerable<CloudElement> list = await controller.GetListAsync();
            var dirApp = list.FirstOrDefault(x => x.IsDirectory && x.DisplayName == APP_DIR);
            if (dirApp == null)
            {
                dirApp = await controller.CreateDirAsync(string.Empty, APP_DIR);
            }

            DirAppHref = dirApp.Href;
            list = await controller.GetListAsync(DirAppHref);
            var dirTable = list.FirstOrDefault(x => x.IsDirectory && x.DisplayName == TABLE_DIR);
            if (dirTable == null)
            {
                dirTable = await controller.CreateDirAsync(DirAppHref, TABLE_DIR);
            }

            DirTableHref = dirTable.Href;
        }
    }
}
