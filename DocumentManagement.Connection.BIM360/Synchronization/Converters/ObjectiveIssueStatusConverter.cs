using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using System;

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
            ObjectiveStatus was;

            if (existing != null)
            {
                was = config.Get
                   .Where(x => x.Source == existing.Attributes.Status)
                   .FirstOrDefault(rule => IsAllMet(rule, existing))
                  ?.Destination ?? ObjectiveStatus.Undefined;

                if (was == objective.Status)
                    return existing.Attributes.Status;
            }

            var newStatus = config.Set.Where(x => x.Source == objective.Status)
               .FirstOrDefault(rule => IsAllMet(rule));
        }

        private static bool IsAllMet(RelationRule<Status, ObjectiveStatus> rule, Issue existing)
        {
            if (rule.Conditions.Any(x => x.ObjectType == ComparisonObjectType.BrioMrs))
                throw new ArgumentException("Can not use Brio MRS values on get");

            return rule.Conditions == null ||
                rule.Conditions.Length == 0 ||
                rule.Conditions.All(x => x.IsMet(existing));
        }

        private static bool IsAllMet(RelationRule<ObjectiveStatus, Status> rule, Issue existing)
            => rule.Conditions == null || rule.Conditions.Length == 0 || rule.Conditions.All(x => x. x.IsMet(existing));
    }
}
