using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    public class LementProConnectionStorage : IConnectionStorage, IDisposable
    {
        private readonly ProjectsService projectsService;

        public LementProConnectionStorage(HttpRequestUtility requestUtility)
            => projectsService = new ProjectsService(requestUtility, new CommonRequestsUtility(requestUtility));

        public void Dispose()
        {
            projectsService.Dispose();
        }

        public Task<bool> DeleteFiles(IEnumerable<ItemExternalDto> itemExternalDto)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DownloadFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDto)
        {
            if (!int.TryParse(projectId, out var parsedId))
                return false;

            var projectFiles = (await projectsService.GetProjectAsync(parsedId)).Values.Files;
            if ((!projectFiles?.Any()) ?? true)
                return true;

            try
            {
                foreach (var item in itemExternalDto)
                {
                    var correspondingModelFile = projectFiles.FirstOrDefault(f => f.FileName == item.FileName);
                    if (correspondingModelFile == default)
                        continue;

                    await projectsService.CommonRequests.DownloadFileAsync(correspondingModelFile.ID.Value, item.FullPath);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
