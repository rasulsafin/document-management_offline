using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    internal class GoogleDriveManager : ICloudManager
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
            this.controller = driveController;
        }

        public string DirAppHref { get; private set; }

        public string DirTableHref { get; private set; }

        public async Task<bool> Push<T>(T @object, string id)
        {
            try
            {
                var tableHref = await GetTableHref<T>();
                string json = JsonConvert.SerializeObject(@object);
                return await controller.SetContentAsync(json, tableHref, id);
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
                var tableHref = await GetTableHref<T>();
                string json = await controller.GetContentAsync(tableHref, id);
                T @object = JsonConvert.DeserializeObject<T>(json);
                return @object;
            }
            catch (Exception ex)
            {
            }

            return default;
        }

        public async Task<bool> Delete<T>(string id)
        {
            var tableHref = await GetTableHref<T>();
            var list = await controller.GetListAsync(tableHref);
            var record = list.FirstOrDefault(x => x.IsDirectory && x.DisplayName == id);
            if (record != null)
            {
                return await controller.DeleteAsync(record.Href);
            }

            return false;
        }

        public async Task<bool> PushFile(string remoteDirName, string localDirName, string fileName)
        {
            string dirHref = await GetDirHref(remoteDirName);
            var res = await controller.LoadFileAsync(dirHref, Path.Combine(localDirName, fileName));
            return res;
        }


        public Task<bool> DeleteFile(string path)
        {
            throw new System.NotImplementedException();
        }


        public Task<bool> PullFile(string remoteDirName, string localDirName, string fileName)
        {
            throw new System.NotImplementedException();
        }



        private async Task<string> GetDirHref(string dirName)
        {
            await CheckDirApp();
            var res = directories.ContainsKey(dirName);
            if (res)
            {
                directories.Clear();
                var list = await controller.GetListAsync(DirAppHref);
                foreach (DiskElement element in list)
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
            if (res)
            {
                tables.Clear();
                var list = await controller.GetListAsync(DirTableHref);
                foreach (DiskElement element in list)
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
            IEnumerable<DiskElement> list = await controller.GetListAsync();
            var dirApp = list.FirstOrDefault(x => x.IsDirectory && x.DisplayName == APP_DIR);
            if (dirApp == null)
            {
                dirApp = await controller.CreateDirAsync("", APP_DIR);
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