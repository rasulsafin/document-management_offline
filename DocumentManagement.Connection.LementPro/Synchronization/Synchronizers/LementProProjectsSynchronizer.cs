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
                .Where(o => o.UpdatedAt != default)
                .Where(o => o.UpdatedAt <= date)
                .Select(o => o.ExternalID).ToList();
        }

        #region NOT IMPLEMENTED METHODS
        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectExternalDto> Update(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }
        #endregion

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
