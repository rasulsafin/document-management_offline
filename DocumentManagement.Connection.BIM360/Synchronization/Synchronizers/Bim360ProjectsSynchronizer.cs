using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    internal class Bim360ProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly Bim360ConnectionContext context;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly SnapshotFiller filler;
        private readonly Authenticator authenticator;

        public Bim360ProjectsSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            SnapshotFiller filler,
            Authenticator authenticator)
        {
            this.context = context;
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

            var cashed = context.Snapshot.ProjectEnumerable.First(x => x.Key == obj.ExternalID);
            var toRemove = cashed.Value.Items.Where(a => obj.Items.All(b => b.ExternalID != a.Value.Entity.ID))
               .ToArray();
            var toAdd = obj.Items.Where(a => cashed.Value.Items.All(b => b.Value.Entity.ID != a.ExternalID)).ToArray();

            foreach (var item in toAdd)
            {
                var posted = await itemsSyncHelper.PostItem(
                    item,
                    context.Snapshot.ProjectEnumerable.First(x => x.Key == obj.ExternalID).Value.ProjectFilesFolder,
                    obj.ExternalID);
                cashed.Value.Items.Add(posted.ID, new ItemSnapshot(posted));
            }

            foreach (var dto in toRemove)
            {
                cashed.Value.Items.Remove(dto.Key);
                await itemsSyncHelper.Remove(obj.ExternalID, dto.Value.Entity);
            }

            return GetFullProject(cashed.Value);
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateHubsIfNull();
            await filler.UpdateProjectsIfNull();
            return context.Snapshot.ProjectEnumerable.Select(x => x.Key).ToList();
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            return (from id in ids
                select context.Snapshot.ProjectEnumerable.FirstOrDefault(x => x.Key == id).Value
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
