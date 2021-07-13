using System;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProConnectionContext : AConnectionContext
    {
        private readonly ProjectsService projectService;
        private readonly ProjectElementsDecorator projectElementsService;
        private readonly IssuesDecorator objectiveService;
        private readonly IConverter<string, (string id, string type)> idConverter;

        public MrsProConnectionContext(ProjectsService projectService, IssuesDecorator objectiveService, ProjectElementsDecorator projectElementsService, IConverter<string, (string id, string type)> idConverter)
        {
            this.projectService = projectService;
            this.projectElementsService = projectElementsService;
            this.idConverter = idConverter;
            this.objectiveService = objectiveService;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            return new MrsProObjectivesSynchronizer(objectiveService, projectElementsService, idConverter);
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            return new MrsProProjectsSynchronizer(projectService);
        }
    }
}
