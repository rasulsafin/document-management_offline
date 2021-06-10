using System;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProConnectionContext : AConnectionContext
    {
        private ProjectsService projectService;
        private IssuesService objectiveService;

        public MrsProConnectionContext(ProjectsService projectService, IssuesService objectiveService)
        {
            this.projectService = projectService;
            this.objectiveService = objectiveService;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            return new MrsProObjectivesSynchronizer(objectiveService, projectService);
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            return new MrsProProjectsSynchronizer(projectService);
        }
    }
}
