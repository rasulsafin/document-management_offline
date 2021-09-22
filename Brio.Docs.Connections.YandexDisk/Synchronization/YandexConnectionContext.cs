using Brio.Docs.Connections.Utils.CloudBase.Synchronizers;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Connections.YandexDisk.Synchronization
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
