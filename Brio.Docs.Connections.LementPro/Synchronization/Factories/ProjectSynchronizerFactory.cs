using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Integration.Factories;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Connections.LementPro.Synchronization.Factories
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
