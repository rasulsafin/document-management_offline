using System;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers
{
    public class ProjectsHelper
    {
        private readonly ProjectsService projectsService;

        public ProjectsHelper(ProjectsService projectsService)
            => this.projectsService = projectsService;

        public async Task<Project> GetProjectAsync(string hubId, Func<Project, bool> projectSelector = null)
        {
            var projects = await projectsService.GetProjectsAsync(hubId);
            var project = projectSelector == null ? projects.FirstOrDefault() : projects.FirstOrDefault(projectSelector);
            return project;
        }
    }
}
