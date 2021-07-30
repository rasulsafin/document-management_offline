using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class EnumerationTypeCreator
    {
        private readonly SnapshotFiller snapshotFiller;
        private readonly Bim360Snapshot snapshot;
        private readonly IssuesService issuesService;

        protected EnumerationTypeCreator(
            SnapshotFiller snapshotFiller,
            Bim360Snapshot snapshot,
            IssuesService issuesService)
        {
            this.snapshotFiller = snapshotFiller;
            this.snapshot = snapshot;
            this.issuesService = issuesService;
        }

        internal async Task<EnumerationTypeExternalDto> Create<T, TID>(IEnumCreator<T, TID> creator, bool canBeNull = false)
        {
            await snapshotFiller.UpdateProjectsIfNull();
            var types = new List<T>();

            foreach (var project in snapshot.ProjectEnumerable)
            {
                try
                {
                    types.AddRange(await creator.GetVariantsFromRemote(issuesService, project.Value));
                }
                catch
                {
                }
            }

            var values = DynamicFieldUtilities.GetGroupedTypes(creator, types)
               .Select(
                    x => new EnumerationValueExternalDto
                    {
                        ExternalID = DynamicFieldUtilities.GetExternalID(creator.GetOrderedIDs(x)),
                        Value = x.Key,
                    });

            if (canBeNull)
            {
                values = values.Append(
                    new EnumerationValueExternalDto
                    {
                        ExternalID = $"{creator.EnumExternalID}_null_value",
                        Value = null,
                    });
            }

            var enumType = new EnumerationTypeExternalDto
            {
                ExternalID = creator.EnumExternalID,
                Name = creator.EnumDisplayName,
                EnumerationValues = values.ToList(),
            };
            return enumType;
        }
    }
}
