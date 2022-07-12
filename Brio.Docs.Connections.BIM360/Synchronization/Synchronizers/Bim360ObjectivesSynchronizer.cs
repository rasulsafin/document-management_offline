using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities.Objective;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly IAccessController accessController;
        private readonly SnapshotFiller filler;
        private readonly ObjectiveGetter objectiveGetter;
        private readonly ObjectiveUpdater objectiveUpdater;
        private readonly ObjectiveRemover objectiveRemover;

        public Bim360ObjectivesSynchronizer(
            IAccessController accessController,
            SnapshotFiller filler,
            ObjectiveGetter objectiveGetter,
            ObjectiveUpdater objectiveUpdater,
            ObjectiveRemover objectiveRemover)
        {
            this.accessController = accessController;
            this.filler = filler;
            this.objectiveGetter = objectiveGetter;
            this.objectiveUpdater = objectiveUpdater;
            this.objectiveRemover = objectiveRemover;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);
            var result = await objectiveUpdater.Put(obj);
            return result;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);
            var result = await objectiveRemover.Remove(obj);
            return result;
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);
            var result = await objectiveUpdater.Put(obj);
            return result;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);
            await UpdateSnapshot(date);
            return await objectiveGetter.GetUpdatedIDs(date).ToListAsync();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
            => await objectiveGetter.Get(ids);

        private async Task UpdateSnapshot(DateTime date)
        {
            await filler.UpdateStatusesConfigIfNull();
            await filler.UpdateIssuesIfNull(date);
            await filler.UpdateIssueTypes();
            await filler.UpdateRootCauses();
            await filler.UpdateLocations();
            await filler.UpdateAssignTo();
            await filler.UpdateStatuses();
        }
    }
}
