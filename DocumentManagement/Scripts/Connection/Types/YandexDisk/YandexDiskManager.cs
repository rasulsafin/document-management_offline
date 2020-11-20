using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.Bim.DocumentManagement.Utilities;
using Disk.SDK;
using Disk.SDK.Provider;

namespace MRS.Bim.DocumentManagement.YandexDisk
{
    public class YandexDiskManager : ICloudManager
    {
        private const int NUMBER_OF_ATTEMPTS = 5;
        private readonly Auth auth = new Auth("YandexApp");
        private IDiskSdkClient yandex;
        private TaskCompletionSource<IEnumerable<DiskItemInfo>> folderContent;
        private TaskCompletionSource<bool> boolResult;

        public async Task<bool> Connect()
        {
            await auth.SignInAsync();
            yandex = new DiskSdkClient(auth.AccessProperty.Token);
            yandex.GetListCompleted += SetFolderItems;
            yandex.MakeFolderCompleted += LoadCompleted;
            return await Task.FromResult(true);
        }

        public void Disconnect()
            => auth.ClearUserInfo();

        public async Task<bool> Download(CloudItem item, string path)
        {
            boolResult = new TaskCompletionSource<bool>();

            using (var fileStream = File.Create(path))
                yandex.DownloadFileAsync(item.ID, fileStream, null, LoadCompleted);
            return await boolResult.Task;
        }

        public async Task<bool> Upload(string path, string parentID, CloudItem item)
        {
            boolResult = new TaskCompletionSource<bool>();
            yandex.UploadFileAsync(parentID + item.Name, File.OpenRead(path), null, LoadCompleted);
            return await boolResult.Task;
        }

        public async Task<List<CloudItem>> GetItems(string parentId = "/", string partOfName = "", bool? folders = null)
        {
            List<CloudItem> result = null;

            for (var i = 0; i < NUMBER_OF_ATTEMPTS && result == null; i++)
            {
                folderContent = new TaskCompletionSource<IEnumerable<DiskItemInfo>>();
                yandex.GetListAsync(parentId);
                result = (await folderContent.Task)?.Select(x => new CloudItem
                {
                    ID = x.FullPath,
                    Name = x.DisplayName,
                    IsFolder = x.IsDirectory,
                    ModifiedTime = x.LastModified,
                    Size = x.ContentLength
                }).Where(x =>
                        x.ID != parentId &&
                        (string.IsNullOrEmpty(partOfName) || x.Name.Contains(partOfName)) &&
                        (!folders.HasValue || x.IsFolder == folders.Value)).ToList();
            }

            return result ?? new List<CloudItem>();
        }

        public async Task<CloudItem> CreateAppDirectory(string appPath)
        {
            boolResult = new TaskCompletionSource<bool>();
            yandex.MakeDirectoryAsync('/' + appPath);

            if (await boolResult.Task)
            {
                return new CloudItem
                {
                    ID = '/' + appPath + '/',
                    IsFolder = true,
                    Name = appPath
                };
            }

            throw new Exception($"Can't create directory {appPath}");
        }

        public void Cancel()
            => auth.Cancel();

        private void SetFolderItems(object sender, GenericSdkEventArgs<IEnumerable<DiskItemInfo>> args)
            => folderContent.SetResult(args.Error == null ? args.Result : null);

        private void LoadCompleted(object sender, SdkEventArgs e)
            => boolResult.SetResult(e.Error == null);
    }
}
