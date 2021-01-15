using MRS.DocumentManagement.Connection.YandexDisk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.GoogleDisk
{

    public class GoogleDiskController : IDiskController
    {
        private string accessToken;

        public GoogleDiskController(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public Task<bool> CopyAsync(string originPath, string copyPath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateDirAsync(string path, string nameDir)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadFileAsync(string href, string currentPath, Action<ulong, ulong> updateProgress = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetContentAsync(string path, Action<ulong, ulong> updateProgress = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DiskElement>> GetListAsync(string path = "/")
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoadFileAsync(string href, string fileName, Action<ulong, ulong> progressChenge = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MoveAsync(string originPath, string movePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetContentAsync(string path, string content, Action<ulong, ulong> progressChenge = null)
        {
            throw new NotImplementedException();
        }
    }
}