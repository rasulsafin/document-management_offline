using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.General.Utils.Factories;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization.Factories
{
    public class ObjectiveSynchronizerFactory : IFactory<LementProConnectionContext, LementProObjectivesSynchronizer>
    {
        private readonly TasksService tasksService;
        private readonly BimsService bimsService;

        public ObjectiveSynchronizerFactory(TasksService tasksService, BimsService bimsService)
        {
            this.tasksService = tasksService;
            this.bimsService = bimsService;
        }

        public LementProObjectivesSynchronizer Create(LementProConnectionContext context)
            => new LementProObjectivesSynchronizer(
                context,
                tasksService,
                bimsService);
    }
}
