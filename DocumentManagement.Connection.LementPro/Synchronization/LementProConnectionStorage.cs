using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            if (!int.TryParse(projectId, out var parsedId))
                return false;

            var remoteProject = await projectsService.GetProjectAsync(parsedId);
            var projectFiles = remoteProject.Values.Files;
            var projectDto = remoteProject.ToProjectExternalDto();

            try
            {
                foreach (var item in itemExternalDtos)
                {
                    if (!projectDto.Items.Any(f => f.ExternalID.Equals(item.ExternalID)))
                        return false;

                    var correspondingItem = projectDto.Items.First(i => i.ExternalID != item.ExternalID);
                    projectDto.Items.Remove(correspondingItem);
                }

                var modelToUpdate = projectDto.ToModelToUpdate(remoteProject);
                modelToUpdate.RemovedFileIds = projectDto.Items.Select(i => int.Parse(i.ExternalID)).ToList();
                await projectsService.UpdateProjectAsync(modelToUpdate);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DownloadFiles(string projectId,
            IEnumerable<ItemExternalDto> itemExternalDto,
            IProgress<double> progress,
            CancellationToken token)
        {
            if (!int.TryParse(projectId, out var parsedId))
            {
                progress?.Report(1.0);
                return false;
            }

            var projectFiles = (await projectsService.GetProjectAsync(parsedId)).Values.Files;
            if ((!projectFiles?.Any()) ?? true)
            {
                progress?.Report(1.0);
                return true;
            }

            try
            {
                int i = 0;
                foreach (var item in itemExternalDto)
                {
                    token.ThrowIfCancellationRequested();
                    var correspondingModelFile = projectFiles.FirstOrDefault(f => f.FileName == item.FileName);
                    if (correspondingModelFile == default)
                        continue;

                    await projectsService.CommonRequests.DownloadFileAsync(correspondingModelFile.ID.Value, item.FullPath);

                    progress?.Report(++i / (double)itemExternalDto.Count());
                }
            }
            catch
            {
                progress?.Report(1.0);
                return false;
            }

            progress?.Report(1.0);
            return true;
        }
    }
}
