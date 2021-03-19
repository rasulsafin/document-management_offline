using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<bool> DownloadFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDto)
        {
            var project = await cloudManager.Pull<ProjectExternalDto>(projectId);
            if (project == default)
                return false;

            foreach (var item in itemExternalDto)
            {
                if (!project.Items.Any(i => i.ExternalID == item.ExternalID))
                    continue;

                var downloadResult = await cloudManager.PullFile(item.ExternalID, item.FullPath);
                if (!downloadResult)
                    return false;
            }

            return true;
        }
    }
}
