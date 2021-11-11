using Brio.Docs.Connections.Bim360.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class EnumerationTypeCreator
    {
        private readonly SnapshotFiller snapshotFiller;
        private readonly Bim360Snapshot snapshot;

        private bool snapshotFilled = false;

        public EnumerationTypeCreator(
            SnapshotFiller snapshotFiller,
            Bim360Snapshot snapshot)
        {
            this.snapshotFiller = snapshotFiller;
            this.snapshot = snapshot;
        }

        internal async Task<EnumerationTypeExternalDto> Create<T, TVariant, TID>(
            IEnumCreator<T, TVariant, TID> creator)
            where TVariant : AEnumVariantSnapshot<T>
        {
            await FillSnapshotIfNotFilled();

            var values = DynamicFieldUtilities.GetGroupedTypes(
                    creator,
                    snapshot.ProjectEnumerable.SelectMany(creator.GetSnapshots))
               .Select(
                    x => new EnumerationValueExternalDto
                    {
                        ExternalID = x.First().ID,
                        Value = x.Key,
                    })
               .ToList();

            if (creator.CanBeNull)
            {
                values.Add(
                    new EnumerationValueExternalDto
                    {
                        ExternalID = creator.NullID,
                        Value = "----------",
                    });
            }

            var enumType = new EnumerationTypeExternalDto
            {
                ExternalID = creator.EnumExternalID,
                Name = creator.EnumDisplayName,
                EnumerationValues = values,
            };
            return enumType;
        }

        private async Task FillSnapshotIfNotFilled()
        {
            if (snapshotFilled)
                return;

            await snapshotFiller.UpdateIssueTypes();
            await snapshotFiller.UpdateRootCauses();
            await snapshotFiller.UpdateLocations();
            await snapshotFiller.UpdateAssignTo();
            await snapshotFiller.UpdateStatuses();
            snapshotFilled = true;
        }
    }
}
