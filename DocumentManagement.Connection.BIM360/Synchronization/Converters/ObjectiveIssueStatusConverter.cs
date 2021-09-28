using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
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

        public async Task<Status> Convert(ObjectiveExternalDto objective)
        {
            Issue existing = null;
            var project = snapshot.ProjectEnumerable.First(x => x.ID == objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                existing = issueSnapshot.Entity;
            var config = project.StatusesRelations ?? IfcConfigUtilities.GetDefaultStatusesConfig();

            if (existing != null &&
                (ConvertByConfig(existing, config) ?? ObjectiveStatus.Undefined) == objective.Status)
                return existing.Attributes.Status;

            var status = ConvertByConfig(objective, config, existing) ?? existing?.Attributes.Status ?? Status.Open;
            if (existing != null && !existing.Attributes.PermittedStatuses.Contains(status))
                status = existing.Attributes.Status;
            return status;
        }

        private static Status? ConvertByConfig(ObjectiveExternalDto objective, StatusesRelations config, Issue existing)
            => config.Set
               .Where(x => x.Source == objective.Status)
               .FirstOrDefault(rule => IsAllMet(rule, existing, objective))
              ?.Destination;

        private static ObjectiveStatus? ConvertByConfig(Issue issue, StatusesRelations config)
            => config.Get
               .Where(x => x.Source == issue.Attributes.Status)
               .FirstOrDefault(rule => IsAllMet(rule, issue))
              ?.Destination;

        private static bool IsAllMet(RelationRule<Status, ObjectiveStatus> rule, Issue issue)
            => rule.Conditions == null ||
                rule.Conditions.Length == 0 ||
                rule.Conditions.All(x => x.IsMet(issue));

        private static bool IsAllMet(
            RelationRule<ObjectiveStatus, Status> rule,
            Issue issue,
            ObjectiveExternalDto objective)
            => rule.Conditions == null || rule.Conditions.Length == 0 ||
                rule.Conditions.All(
                    x => x.ObjectType == ComparisonObjectType.Bim360 ? x.IsMet(issue) : x.IsMet(objective));
    }
}
