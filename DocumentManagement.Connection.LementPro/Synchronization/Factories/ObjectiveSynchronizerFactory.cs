using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.General.Utils.Factories;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization.Factories
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
