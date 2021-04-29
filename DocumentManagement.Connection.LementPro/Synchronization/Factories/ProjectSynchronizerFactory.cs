using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.General.Utils.Factories;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization.Factories
{
    public class ProjectSynchronizerFactory : IFactory<LementProConnectionContext, LementProProjectsSynchronizer>
    {
        private readonly ProjectsService projectsService;
        private readonly ILoggerFactory loggerFactory;

        public ProjectSynchronizerFactory(ProjectsService projectsService, ILoggerFactory loggerFactory)
        {
            this.projectsService = projectsService;
            this.loggerFactory = loggerFactory;
        }

        public LementProProjectsSynchronizer Create(LementProConnectionContext context)
            => new LementProProjectsSynchronizer(
                context,
                projectsService,
                loggerFactory.CreateLogger<LementProProjectsSynchronizer>());
    }
}
