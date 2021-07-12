using System;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProConnectionContext : AConnectionContext
    {
        private readonly ProjectsService projectService;
        private readonly ProjectElementsService projectElementsService;
        private readonly IssuesService objectiveService;

        public MrsProConnectionContext(ProjectsService projectService, IssuesService objectiveService, ProjectElementsService projectElementsService)
        {
            this.projectService = projectService;
            this.projectElementsService = projectElementsService;
            this.objectiveService = objectiveService;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            return new MrsProObjectivesSynchronizer(objectiveService, projectElementsService);
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            return new MrsProProjectsSynchronizer(projectService);
        }
    }
}
