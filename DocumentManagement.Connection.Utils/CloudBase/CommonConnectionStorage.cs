using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Utils.CloudBase
{
    public class CommonConnectionStorage : IConnectionStorage
    {
        private ICloudManager cloudManager;

        public CommonConnectionStorage(ICloudManager cloudManager)
            =>  this.cloudManager = cloudManager;

        public Task<bool> DeleteFiles(IEnumerable<ItemExternalDto> itemExternalDto)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DownloadFiles(string projectId,
            IEnumerable<ItemExternalDto> itemExternalDto,
            IProgress<double> progress,
            CancellationToken token)
        {
            int i = 0;
            foreach (var item in itemExternalDto)
            {
                token.ThrowIfCancellationRequested();
                var downloadResult = await cloudManager.PullFile(item.ExternalID, item.FullPath);
                progress?.Report(++i / (double)itemExternalDto.Count());
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
