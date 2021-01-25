#define TEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection
{
    public class DiskManager : IDiskManager
    {
        // public static CoolLogger logger = new CoolLogger(typeof(DiskManager).Name);

        private string accessToken;
        private IDiskController controller;
        private bool projectsDirCreate;
        private bool transactionsDirCreate;
        private bool usersDirCreate;
        private List<int> projects = new List<int>();
        private List<int> objectives = new List<int>();
        private List<int> items = new List<int>();
        private List<string> tables = new List<string>();
        private bool isInit;

        public DiskManager(string accessToken)
        {
            this.accessToken = accessToken;
            controller = new YandexDiskController(accessToken);
        }

        public DiskManager(IDiskController controller)
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

        public async Task Delete<T>(string id)
        {            
            if (await CheckTableDir<T>())
            {
                string tableName = typeof(T).Name;
                string path = PathManager.GetRecordFile(tableName, id);
                await controller.DeleteAsync(path);
            }
        }

        private async Task<bool> CheckTableDir<T>()
        {
            if (!isInit) await Initialize();
            string tableName = typeof(T).Name;
            bool res = tables.Any(x => x == tableName);
            if (res) return true;
            IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetTablesDir());
            foreach (DiskElement element in list)
            {
                if (element.IsDirectory)
                    tables.Add(element.DisplayName);
                if (element.DisplayName == tableName)
                    res = true;
            }

            if (!res)
                await controller.CreateDirAsync(PathManager.GetTablesDir(), tableName);
            return res;
        }

        private async Task Initialize()
        {
            IEnumerable<DiskElement> list = await controller.GetListAsync();
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

    }
}
