using MRS.DocumentManagement.Connection.YandexDisk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection
{
    public interface IDiskController
    {
        Task<bool> CopyAsync(string originPath, string copyPath);
        Task<bool> CreateDirAsync(string path, string nameDir);
        Task<bool> DeleteAsync(string path);
        Task<bool> DownloadFileAsync(string href, string currentPath, Action<ulong, ulong> updateProgress = null);
        Task<string> GetContentAsync(string path, Action<ulong, ulong> updateProgress = null);
        Task<IEnumerable<DiskElement>> GetListAsync(string path = "/");
        Task<bool> LoadFileAsync(string href, string fileName, Action<ulong, ulong> progressChenge = null);
        Task<bool> MoveAsync(string originPath, string movePath);
        Task<bool> SetContentAsync(string path, string content, Action<ulong, ulong> progressChenge = null);
    }
}