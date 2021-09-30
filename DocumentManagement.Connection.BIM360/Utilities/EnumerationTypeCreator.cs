using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
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
                    creator.GetSnapshots(snapshot.ProjectEnumerable))
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
            snapshotFilled = true;
        }
    }
}
