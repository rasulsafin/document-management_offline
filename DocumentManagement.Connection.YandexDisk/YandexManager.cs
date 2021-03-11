#define TEST

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection
{
    public class YandexManager : ICloudManager
    {
        private string accessToken;
        private YandexDiskController controller;
        private List<string> tables = new List<string>();
        private List<string> directories = new List<string>();
        private bool isInit;

        public YandexManager(YandexDiskController controller)
        {
            this.controller = controller;
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
            catch (Exception ex)
            {
            }

            return false;
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

        public async Task<bool> DeleteFile(string path)
        {
            return await controller.DeleteAsync(path);
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

        public async Task<ConnectionStatusDto> GetStatusAsync()
        {
            if (controller == null)
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.NeedReconnect, Message = "Controller null" };
            try
            {
                var list = await controller.GetListAsync(PathManager.GetAppDir());
                if (list != null)
                    return new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Good" };
            }
            catch (TimeoutException)
            {
            }

            return new ConnectionStatusDto() { Status = RemoteConnectionStatus.NeedReconnect, Message = "No network" };
        }

        public async Task<string> PushFile(string remoteDirName, string localDirName, string fileName)
        {
            try
            {
                await CheckDir(remoteDirName);
                string path = PathManager.GetDir(remoteDirName);
                string file = Path.Combine(localDirName, fileName);
                await controller.LoadFileAsync(path, file);
                return path;
            }
            catch (Exception ex)
            {
            }

            return string.Empty;
        }

        private async Task<bool> CheckTableDir<T>()
        {
            if (!isInit) await Initialize();
            string tableName = typeof(T).Name;
            bool result = tables.Any(x => x == tableName);
            if (result)
                return true;
            IEnumerable<CloudElement> list = await controller.GetListAsync(PathManager.GetTablesDir());
            foreach (CloudElement element in list)
            {
                if (element.IsDirectory)
                    tables.Add(element.DisplayName);
                if (element.DisplayName == tableName)
                    result = true;
            }

            if (!result)
                await controller.CreateDirAsync(PathManager.GetTablesDir(), tableName);
            return result;
        }

        private async Task Initialize()
        {
            IEnumerable<CloudElement> list = await controller.GetListAsync();
            if (!list.Any(x => x.IsDirectory && x.DisplayName == PathManager.APP_DIR))
            {
                await controller.CreateDirAsync("/", PathManager.APP_DIR);
            }

            list = await controller.GetListAsync(PathManager.GetAppDir());
            if (!list.Any(x => x.IsDirectory && x.DisplayName == PathManager.TABLE_DIR))
            {
                await controller.CreateDirAsync("/", PathManager.GetTablesDir());
            }

            isInit = true;
        }

        private async Task<bool> CheckDir(string dirName)
        {
            bool res = directories.Any(x => x == dirName);
            if (res) return true;
            IEnumerable<CloudElement> list = await controller.GetListAsync(PathManager.GetAppDir());
            foreach (CloudElement element in list)
            {
                if (element.IsDirectory)
                    directories.Add(element.DisplayName);
                if (element.DisplayName == dirName)
                    res = true;
            }

            if (!res)
                await controller.CreateDirAsync(PathManager.GetAppDir(), dirName);
            return res;
        }
    }
}
