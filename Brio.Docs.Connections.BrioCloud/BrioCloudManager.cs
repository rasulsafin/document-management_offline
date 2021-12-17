using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.External;
using Brio.Docs.External.Utils;

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

        public Task<bool> Push<T>(T @object, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete<T>(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CloudElement>> GetRemoteDirectoryFiles(string directoryPath = "/")
        {
            throw new NotImplementedException();
        }

        public Task<T> Pull<T>(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> PullAll<T>(string path)
        {
            throw new NotImplementedException();
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
    }
}
