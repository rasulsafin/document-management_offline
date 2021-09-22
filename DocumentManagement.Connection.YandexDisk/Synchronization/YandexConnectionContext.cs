using Brio.Docs.Connection.Utils.CloudBase.Synchronizers;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Connection.YandexDisk.Synchronization
{
    public class YandexConnectionContext : AConnectionContext
    {
        private YandexManager manager;

        private YandexConnectionContext()
        {
        }

        public static YandexConnectionContext CreateContext(YandexManager manager)
        {
            var context = new YandexConnectionContext { manager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new StorageObjectiveSynchronizer(manager);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new StorageProjectSynchronizer(manager);
    }
}
