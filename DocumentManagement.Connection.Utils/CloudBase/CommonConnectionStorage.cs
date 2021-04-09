using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils.CloudBase.Synchronizers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Utils.CloudBase
{
    public class CommonConnectionStorage : IConnectionStorage
    {
        private ICloudManager cloudManager;

        public CommonConnectionStorage(ICloudManager cloudManager)
            =>  this.cloudManager = cloudManager;

        public async Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            var projectFiles = ItemsSyncHelper.GetProjectItems(projectId, cloudManager);
            var deletionResult = true;
            foreach (var item in itemExternalDtos)
            {
                if (!(await projectFiles).Any(f => f.ExternalID.Equals(item.ExternalID)))
                    return false;

                if (!string.IsNullOrWhiteSpace(item?.ExternalID))
                    deletionResult = await cloudManager.DeleteFile(item.ExternalID) && deletionResult;
            }

            return deletionResult;
        }

        public async Task<bool> DownloadFiles(string projectId,
            IEnumerable<ItemExternalDto> itemExternalDtos,
            IProgress<double> progress,
            CancellationToken token)
        {
            int i = 0;
            foreach (var item in itemExternalDtos)
            {
                token.ThrowIfCancellationRequested();
                var downloadResult = await cloudManager.PullFile(item.ExternalID, item.FullPath);
                progress?.Report(++i / (double)itemExternalDtos.Count());
                if (!downloadResult)
                {
                    progress?.Report(1.0);
                    return false;
                }
            }

            progress?.Report(1.0);
            return true;
        }
    }
}
