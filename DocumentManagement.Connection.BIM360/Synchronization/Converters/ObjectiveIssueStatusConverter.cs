using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueStatusConverter : IConverter<ObjectiveExternalDto, Status>
    {
        private readonly Bim360Snapshot snapshot;

        public ObjectiveIssueStatusConverter(Bim360Snapshot snapshot)
            => this.snapshot = snapshot;

        public Task<Status> Convert(ObjectiveExternalDto objective)
        {
            Issue existing = null;
            var project = snapshot.ProjectEnumerable.First(x => x.ID == objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                existing = issueSnapshot.Entity;
            var config = project.StatusesRelations ?? IfcConfigUtilities.GetDefaultStatusesConfig();

            if (existing != null &&
                existing.GetSuitableStatuses(config).Append(ObjectiveStatus.Undefined).First() == objective.Status)
                return Task.FromResult(existing.Attributes.Status);

            var statuses = objective.GetSuitableStatuses(config, existing);

            if (existing != null)
            {
                foreach (var status in statuses)
                {
                    if (existing.Attributes.PermittedStatuses.Contains(status))
                        return Task.FromResult(status);
                }

                return Task.FromResult(existing.Attributes.Status);
            }

            return Task.FromResult(statuses.Append(Status.Open).First(x => x is Status.Draft or Status.Open));
        }
    }
}
