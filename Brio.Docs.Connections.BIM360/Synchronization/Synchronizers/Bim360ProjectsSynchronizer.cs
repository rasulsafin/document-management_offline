using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronizers
{
    internal class Bim360ProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly Bim360Snapshot snapshot;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly SnapshotFiller filler;
        private readonly Authenticator authenticator;

        public Bim360ProjectsSynchronizer(
            Bim360Snapshot snapshot,
            ItemsSyncHelper itemsSyncHelper,
            SnapshotFiller filler,
            Authenticator authenticator)
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

        public async Task<ProjectExternalDto> Update(ProjectExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var cashed = snapshot.ProjectEnumerable.First(x => x.ID == obj.ExternalID);
            var toRemove = cashed.Items.Where(a => obj.Items.All(b => b.ExternalID != a.Value.Entity.ID))
               .ToArray();
            var toAdd = obj.Items.Where(a => cashed.Items.All(b => b.Value.Entity.ID != a.ExternalID)).ToArray();

            foreach (var item in toAdd)
                await itemsSyncHelper.PostItem(cashed, item);

            foreach (var dto in toRemove)
            {
                cashed.Items.Remove(dto.Key);
                await itemsSyncHelper.Remove(obj.ExternalID, dto.Value.Entity);
            }

            return GetFullProject(cashed);
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateProjectsIfNull();
            return snapshot.ProjectEnumerable.Select(x => x.ID).ToList();
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            return (from id in ids
                select snapshot.ProjectEnumerable.FirstOrDefault(x => x.ID == id)
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
