using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.Bim.DocumentManagement.Utilities
{
    public interface ICloudManager
    {
        Task<bool> Connect();
        void Disconnect();
        Task<bool> Download(CloudItem item, string path);
        Task<bool> Upload(string path, string parentID, CloudItem item);
        Task<List<CloudItem>> GetItems(string parentID = "", string partOfName = "", bool? needFolders = null);
        Task<CloudItem> CreateAppDirectory(string appPath);
        void Cancel();
    }
}

