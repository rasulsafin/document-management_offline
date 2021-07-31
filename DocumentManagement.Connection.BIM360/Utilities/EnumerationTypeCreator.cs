using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class EnumerationTypeCreator
    {
        private static readonly string NULL_VALUE_ID = "_null_value";
        private readonly SnapshotFiller snapshotFiller;
        private readonly Bim360Snapshot snapshot;

        private bool snapshotFilled = false;

        protected EnumerationTypeCreator(
            SnapshotFiller snapshotFiller,
            Bim360Snapshot snapshot)
        {
            this.snapshotFiller = snapshotFiller;
            this.snapshot = snapshot;
        }

        internal static bool IsNullValue(string id)
            => id.EndsWith(NULL_VALUE_ID);

        internal async Task<EnumerationTypeExternalDto> Create<T, TVariant, TID>(
            IEnumCreator<T, TVariant, TID> creator,
            bool canBeNull = false)
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

            if (canBeNull)
            {
                values.Add(
                    new EnumerationValueExternalDto
                    {
                        ExternalID = $"{creator.EnumExternalID}{NULL_VALUE_ID}",
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
