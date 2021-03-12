using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization
{
    public class YandexConnectionContext : AConnectionContext
    {
        private YandexConnectionContext()
        {
        }

        internal YandexManager YandexManager { get; set; }

        public static YandexConnectionContext CreateContext(YandexManager manager)
        {
            var context = new YandexConnectionContext { YandexManager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new YandexObjectivesSynchronizer(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new YandexProjectsSynchronizer(this);
    }
}
