using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    public class LementProProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectsService projectsService;

        private List<ProjectExternalDto> projects;

        public LementProProjectsSynchronizer(LementProConnectionContext context)
            => projectsService = context.ProjectsService;

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await CheckCashedElements();
            return projects.Where(o => ids.Contains(o.ExternalID)).ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await CheckCashedElements();
            return projects
                .Where(o => o.UpdatedAt <= date)
                .Select(o => o.ExternalID).ToList();
        }

        public async Task<ProjectExternalDto> Add(ProjectExternalDto obj)
        {
            var defaultProjectType = projectsService.GetDefaultProjectTypeAsync();
            var newProject = obj.ToModelToCreate();
            newProject.Values.Type = (await defaultProjectType).ID;
            var createResult = await projectsService.CreateProjectAsync(newProject);
            if (!createResult.IsSuccess.GetValueOrDefault())
                return null;

            // Wait for creating
            await Task.Delay(3000);

            var created = await projectsService.GetProjectAsync(createResult.ID.Value);

            return created.ToProjectExternalDto();
        }

        public async Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
        {
            if (!int.TryParse(obj.ExternalID, out var parsedId))
                return null;

            var deleted = await projectsService.DeleteProjectAsync(parsedId);
            return deleted.ToProjectExternalDto();
        }

        public async Task<ProjectExternalDto> Update(ProjectExternalDto obj)
        {
            if (!int.TryParse(obj.ExternalID, out var parsedId))
                return null;

            var existingProjectsObject = await projectsService.GetProjectAsync(parsedId);
            var modelToUpdate = obj.ToModelToUpdate(existingProjectsObject);
            var updated = await projectsService.UpdateProjectAsync(modelToUpdate);

            return updated.ToProjectExternalDto();
        }

        private async Task CheckCashedElements()
        {
            if (projects == null)
            {
                projects = new List<ProjectExternalDto>
                {
                    DEFAULT_PROJECT_STUB,
                };

                var modelProjects = await projectsService.GetAllProjectsAsync();

                foreach (var model in modelProjects)
                {
                    // It is necessary to get full info about issue to get last updated info
                    var fullInfoProject = await projectsService.GetProjectAsync(model.ID.Value);
                    var parsedModel = fullInfoProject.ToProjectExternalDto();
                    projects.Add(parsedModel);
                }
            }
        }
    }
}
