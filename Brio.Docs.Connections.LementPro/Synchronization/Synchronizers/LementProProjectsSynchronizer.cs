using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Brio.Docs.Connections.LementPro.LementProConstants;

namespace Brio.Docs.Connections.LementPro.Synchronization
{
    // TODO: use capture from context.
    public class LementProProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectsService projectsService;
        private readonly ILogger<LementProProjectsSynchronizer> logger;

        private List<ProjectExternalDto> projects;

        public LementProProjectsSynchronizer(
            LementProConnectionContext context,
            ProjectsService projectsService,
            ILogger<LementProProjectsSynchronizer> logger)
        {
            this.projectsService = projectsService;
            this.logger = logger;
            logger.LogTrace("LementProProjectsSynchronizer created");
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            logger.LogTrace("Get started with ids: {@IDs}", ids);
            await CheckCashedElements();
            return projects.Where(o => ids.Contains(o.ExternalID)).ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            logger.LogTrace("GetUpdatedIDs started with date: {@Date}", date);
            await CheckCashedElements();
            return projects
                .Where(o => o.UpdatedAt >= date)
                .Select(o => o.ExternalID).ToList();
        }

        public async Task<ProjectExternalDto> Add(ProjectExternalDto project)
        {
            logger.LogTrace("Add started with project: {@Project}", project);
            var defaultProjectType = projectsService.GetDefaultProjectTypeAsync();
            logger.LogDebug("Received project type: {@ProjectType}", defaultProjectType);
            var newProject = project.ToModelToCreate();
            logger.LogDebug("Mapped project: {@Project}", newProject);
            var isItemsAdding = project.Items?.Any() ?? false;

            if (isItemsAdding)
            {
                var fileIds = await ItemsSynchronizationHelper.UploadFilesAsync(project.Items, projectsService.CommonRequests);
                newProject.FileIds = fileIds;
            }

            newProject.Values.Type = (await defaultProjectType).ID;
            var createResult = await projectsService.CreateProjectAsync(newProject);
            logger.LogDebug("Created project: {@Project}", newProject);
            if (!createResult.IsSuccess.GetValueOrDefault())
                return null;

            // Wait for creating
            await Task.Delay(3000);

            var created = await projectsService.GetProjectAsync(createResult.ID.Value);
            logger.LogDebug("Received project: {@Project}", newProject);
            var parsedToDto = created.ToProjectExternalDto();

            if (isItemsAdding)
                parsedToDto.Items = created.Values.Files?.ToDtoItems(project.Items);
            logger.LogDebug("Mapped project: {@Project}", parsedToDto);

            return parsedToDto;
        }

        public async Task<ProjectExternalDto> Remove(ProjectExternalDto project)
        {
            logger.LogTrace("Remove started with project: {@Project}", project);
            if (!int.TryParse(project.ExternalID, out var parsedId))
                return null;

            var deleted = await projectsService.DeleteProjectAsync(parsedId);
            logger.LogDebug("Deleted project: {@Project}", deleted);
            return deleted.ToProjectExternalDto();
        }

        public async Task<ProjectExternalDto> Update(ProjectExternalDto project)
        {
            logger.LogTrace("Update started with project: {@Project}", project);
            var isItemsAdding = project.Items?.Any() ?? false;
            if (!int.TryParse(project.ExternalID, out var parsedId))
                return null;

            var existingProjectsModel = await projectsService.GetProjectAsync(parsedId);
            logger.LogDebug("Received project: {@Project}", existingProjectsModel);
            var modelToUpdate = project.ToModelToUpdate(existingProjectsModel);

            if (isItemsAdding)
            {
                modelToUpdate = await ItemsSynchronizationHelper
                    .UpdateFilesAsync(existingProjectsModel, modelToUpdate, project.Items, projectsService.CommonRequests);
            }

            logger.LogDebug("Mapped project: {@Project}", modelToUpdate);
            var updated = await projectsService.UpdateProjectAsync(modelToUpdate);
            logger.LogDebug("Updated project: {@Project}", updated);
            var parsedResult = updated.ToProjectExternalDto();

            if (isItemsAdding)
                parsedResult.Items = updated.Values.Files.ToDtoItems(project.Items);
            logger.LogDebug("Mapped project: {@Project}", parsedResult);

            return parsedResult;
        }

        private async Task CheckCashedElements()
        {
            logger.LogTrace("CheckCashedElements started");
            if (projects == null)
            {
                projects = new List<ProjectExternalDto>
                {
                    DEFAULT_PROJECT_STUB,
                };

                var modelProjects = await projectsService.GetAllProjectsAsync();
                logger.LogDebug("Received projects: {@Projects}", modelProjects);

                foreach (var model in modelProjects)
                {
                    // It is necessary to get full info about issue to get last updated info
                    var fullInfoProject = await projectsService.GetProjectAsync(model.ID.Value);
                    logger.LogDebug("Received project: {@Project}", fullInfoProject);
                    var parsedModel = fullInfoProject.ToProjectExternalDto();
                    logger.LogDebug("Mapped project: {@Project}", parsedModel);
                    projects.Add(parsedModel);
                }
            }
        }
    }
}
