using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Connections.LementPro.Synchronization
{
    public class LementProConnectionStorage : IConnectionStorage
    {
        private readonly ProjectsService projectsService;
        private readonly ILogger<LementProConnectionStorage> logger;

        public LementProConnectionStorage(ProjectsService projectsService, ILogger<LementProConnectionStorage> logger)
        {
            this.projectsService = projectsService;
            this.logger = logger;
            logger.LogTrace("LementProConnectionStorage created");
        }

        public async Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            logger.LogTrace(
                "DeleteFiles started with projectId: {@ProjectID}, itemExternalDtos: {@Items}",
                projectId,
                itemExternalDtos);
            if (!int.TryParse(projectId, out var parsedId))
                return false;

            var remoteProject = await projectsService.GetProjectAsync(parsedId);
            logger.LogDebug("Received project: {@Project}", remoteProject);
            var projectFiles = remoteProject.Values.Files;
            var projectDto = remoteProject.ToProjectExternalDto();
            logger.LogDebug("Mapped project: {@Project}", projectDto);

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
                logger.LogDebug("Mapped project: {@Project}", modelToUpdate);
                modelToUpdate.RemovedFileIds = projectDto.Items.Select(i => int.Parse(i.ExternalID)).ToList();
                await projectsService.UpdateProjectAsync(modelToUpdate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't delete file");
                return false;
            }

            return true;
        }

        public async Task<bool> DownloadFiles(string projectId,
            IEnumerable<ItemExternalDto> itemExternalDto,
            IProgress<double> progress,
            CancellationToken token)
        {
            logger.LogTrace("DownloadFiles started with projectId: {@ProjectID}, itemExternalDto: {@Item}", projectId, itemExternalDto);
            if (!int.TryParse(projectId, out var parsedId))
            {
                logger.LogError("Invalid project ID: {@ProjectID}", projectId);
                progress?.Report(1.0);
                return false;
            }

            var projectFiles = (await projectsService.GetProjectAsync(parsedId)).Values.Files;
            logger.LogDebug("Received files: {@Items}", projectFiles);
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

                    await projectsService.CommonRequests.DownloadFileAsync(
                        correspondingModelFile.ID.Value,
                        item.FullPath);

                    progress?.Report(++i / (double)itemExternalDto.Count());
                }
            }
            catch (OperationCanceledException)
            {
                progress?.Report(1.0);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Download failed with projectId: {@ProjectID}, itemExternalDto: {@Item}",
                    projectId,
                    itemExternalDto);
                progress?.Report(1.0);
                return false;
            }

            progress?.Report(1.0);
            return true;
        }
    }
}
