using Brio.Docs.Connection.LementPro.Services;
using Brio.Docs.General.Utils.Factories;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Connection.LementPro.Synchronization.Factories
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
