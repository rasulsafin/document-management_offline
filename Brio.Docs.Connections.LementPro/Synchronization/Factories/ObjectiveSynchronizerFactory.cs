using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.General.Utils.Factories;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Connections.LementPro.Synchronization.Factories
{
    public class ObjectiveSynchronizerFactory : IFactory<LementProConnectionContext, LementProObjectivesSynchronizer>
    {
        private readonly TasksService tasksService;
        private readonly BimsService bimsService;
        private readonly ILoggerFactory loggerFactory;

        public ObjectiveSynchronizerFactory(TasksService tasksService, BimsService bimsService, ILoggerFactory loggerFactory)
        {
            this.tasksService = tasksService;
            this.bimsService = bimsService;
            this.loggerFactory = loggerFactory;
        }

        public LementProObjectivesSynchronizer Create(LementProConnectionContext context)
            => new LementProObjectivesSynchronizer(
                context,
                tasksService,
                bimsService,
                loggerFactory.CreateLogger<LementProObjectivesSynchronizer>());
    }
}
