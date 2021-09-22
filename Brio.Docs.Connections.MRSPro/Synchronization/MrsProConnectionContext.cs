using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.MrsPro
{
    public class MrsProConnectionContext : AConnectionContext
    {
        private readonly ProjectsDecorator projectService;

        // TODO: Fix this list
        private readonly ProjectElementsDecorator projectElementsService;
        private readonly IssuesDecorator objectiveService;
        private readonly IConverter<string, (string id, string type)> idConverter;

        public MrsProConnectionContext(ProjectsDecorator projectService,
            IssuesDecorator objectiveService,
            ProjectElementsDecorator projectElementsService,
            IConverter<string, (string id, string type)> idConverter)
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
