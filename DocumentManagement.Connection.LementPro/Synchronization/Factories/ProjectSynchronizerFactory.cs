using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.General.Utils.Factories;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization.Factories
{
    public class ProjectSynchronizerFactory : IFactory<LementProConnectionContext, LementProProjectsSynchronizer>
    {
        private readonly ProjectsService projectsService;

        public ProjectSynchronizerFactory(ProjectsService projectsService)
            => this.projectsService = projectsService;

        public LementProProjectsSynchronizer Create(LementProConnectionContext context)
            => new LementProProjectsSynchronizer(
                context,
                projectsService);
    }
}
