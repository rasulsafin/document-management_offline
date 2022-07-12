using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronizers
{
    internal class Bim360ProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly SnapshotGetter snapshot;
        private readonly IItemsUpdater itemsSyncHelper;
        private readonly SnapshotFiller filler;
        private readonly IAccessController authenticator;

        public Bim360ProjectsSynchronizer(
            SnapshotGetter snapshot,
            IItemsUpdater itemsSyncHelper,
            SnapshotFiller filler,
            IAccessController authenticator)
        {
            this.snapshot = snapshot;
            this.itemsSyncHelper = itemsSyncHelper;
            this.filler = filler;
            this.authenticator = authenticator;
        }

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
            => throw new NotSupportedException();

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
            => throw new NotSupportedException();

        public async Task<ProjectExternalDto> Update(ProjectExternalDto project)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var cached = snapshot.GetProject(project.ExternalID);
            var toRemove = cached.Items.Where(a => project.Items.All(b => b.ExternalID != a.Value.Entity.ID))
               .ToArray();
            var toAdd = project.Items.Where(a => cached.Items.All(b => b.Value.Entity.ID != a.ExternalID)).ToArray();

            foreach (var item in toAdd)
                await itemsSyncHelper.PostItem(cached, item.FullPath);

            foreach (var itemSnapshot in toRemove.Select(x => x.Value))
                await itemsSyncHelper.Remove(cached, itemSnapshot.ID);

            return GetFullProject(cached);
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateProjectsIfNull();
            await filler.UpdateProjectsInfoIfNull();
            return snapshot.GetProjects().Select(x => x.ID).ToList();
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            return (from id in ids
                select snapshot.GetProject(id)
                into project
                where project != null
                select GetFullProject(project)).ToList();
        }

        private ProjectExternalDto GetFullProject(ProjectSnapshot project)
        {
            var dto = project.Entity.ToDto();
            dto.Items = project.Items.Select(x => x.Value.Entity.ToDto()).ToList();
            return dto;
        }
    }
}
