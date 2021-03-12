using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
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

        public static YandexConnectionContext CreateContext(
            DateTime lastSynchronizationDate,
            YandexManager manager)
        {
            var context = new YandexConnectionContext { YandexManager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new YandexObjectivesSynchronizer(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new YandexProjectsSynchronizer(this);

        protected async override Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives()
            => await YandexManager.PullAll<ObjectiveExternalDto>(PathManager.GetTableDir(nameof(ObjectiveExternalDto)));

        protected async override Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects()
            => await YandexManager.PullAll<ProjectExternalDto>(PathManager.GetTableDir(nameof(ProjectExternalDto)));
    }
}
