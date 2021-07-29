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

        internal async Task<EnumerationTypeExternalDto> Create<T, TID>(IDFHelper<T, TID> helper, bool canBeNull = false)
        {
            await snapshotFiller.UpdateProjectsIfNull();
            var types = new List<T>();

            foreach (var project in snapshot.ProjectEnumerable)
            {
                try
                {
                    types.AddRange(await helper.GetFromRemote(issuesService, project.Value));
                }
                catch
                {
                }
            }

            var values = DynamicFieldUtilities.GetGroupedTypes(helper, types)
               .Select(
                    x => new EnumerationValueExternalDto
                    {
                        ExternalID = DynamicFieldUtilities.GetExternalID(helper.Order(x)),
                        Value = x.Key,
                    });

            if (canBeNull)
            {
                values = values.Append(
                    new EnumerationValueExternalDto
                    {
                        ExternalID = $"{helper.ID}_null_value",
                        Value = null,
                    });
            }

            var enumType = new EnumerationTypeExternalDto
            {
                ExternalID = helper.ID,
                Name = helper.DisplayName,
                EnumerationValues = values.ToList(),
            };
            return enumType;
        }
    }
}
