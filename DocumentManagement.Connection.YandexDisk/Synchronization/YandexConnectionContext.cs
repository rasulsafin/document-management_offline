using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronization.Synchronizers;
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

        protected override Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives()
        {
            throw new NotImplementedException();
        }

        protected override Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects()
        {
            throw new NotImplementedException();
        }
    }
}
