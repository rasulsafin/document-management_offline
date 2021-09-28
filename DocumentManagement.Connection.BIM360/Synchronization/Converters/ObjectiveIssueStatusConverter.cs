using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Exceptions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Interfaces.StatusRelations;
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

        public Task<Status> Convert(ObjectiveExternalDto objective)
        {
            Issue existing = null;
            var project = snapshot.ProjectEnumerable.First(x => x.ID == objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                existing = issueSnapshot.Entity;
            var config = project.StatusesRelations ?? IfcConfigUtilities.GetDefaultStatusesConfig();

            if (existing != null &&
                (ConvertByConfig(existing, config) ?? ObjectiveStatus.Undefined) == objective.Status)
                return Task.FromResult(existing.Attributes.Status);

            var status = ConvertByConfig(objective, config, existing) ?? existing?.Attributes.Status ?? Status.Open;
            if (existing != null && !existing.Attributes.PermittedStatuses.Contains(status))
                status = existing.Attributes.Status;
            return Task.FromResult(status);
        }

        private static Status? ConvertByConfig(ObjectiveExternalDto objective, StatusesRelations config, Issue existing = null)
        {
            for (var i = 0; i < config.Set.Length; i++)
            {
                var relationRule = config.Set[i];

                if (relationRule.Source != objective.Status)
                    continue;

                bool isAllMet;

                try
                {
                    isAllMet = IsAllMet(relationRule, existing, objective);
                }
                catch (ConfigIncorrectException exception)
                {
                    throw new ConfigIncorrectException(
                        "Set contains incorrect rule",
                        DataMemberUtilities.GetPath<StatusesRelations>(x => x.Set),
                        i,
                        exception);
                }

                if (isAllMet)
                    return relationRule.Destination;
            }

            return null;
        }

        private static ObjectiveStatus? ConvertByConfig(Issue issue, StatusesRelations config)
        {
            for (var i = 0; i < config.Get.Length; i++)
            {
                var relationRule = config.Get[i];

                if (relationRule.Source != issue.Attributes.Status)
                    continue;

                bool isAllMet;

                try
                {
                    isAllMet = IsAllMet(relationRule, issue);
                }
                catch (ConfigIncorrectException exception)
                {
                    throw new ConfigIncorrectException(
                        "Get contains incorrect rule",
                        DataMemberUtilities.GetPath<StatusesRelations>(x => x.Get),
                        i,
                        exception);
                }

                if (isAllMet)
                    return relationRule.Destination;
            }

            return null;
        }

        private static bool IsAllMet(IRelationRule rule, Issue issue)
        {
            CheckRule(rule);

            return rule.Conditions == null ||
                rule.Conditions.Length == 0 ||
                rule.Conditions.All(x => x.IsMet(issue));
        }

        private static bool IsAllMet(
            IRelationRule rule,
            Issue issue,
            ObjectiveExternalDto objective)
        {
            CheckRule(rule);

            return rule.Conditions == null || rule.Conditions.Length == 0 ||
                rule.Conditions.All(
                    x => x.ObjectType == ComparisonObjectType.Bim360 ? x.IsMet(issue) : x.IsMet(objective));
        }

        private static void CheckRule(IRelationRule rule)
        {
            if (rule.Conditions != null &&
                rule.Conditions.Any(x => x.ComparisonType == RelationComparisonType.Undefined))
            {
                throw new ConfigIncorrectException(
                    "Conditions contains incorrect comparison type",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.ComparisonType),
                    rule.Conditions.IndexOfFirst(x => x.ComparisonType == RelationComparisonType.Undefined));
            }

            if (rule.Conditions != null &&
                rule.Conditions.Any(x => x.ObjectType == ComparisonObjectType.Undefined))
            {
                throw new ConfigIncorrectException(
                    "Conditions contains incorrect object type",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.ObjectType),
                    rule.Conditions.IndexOfFirst(x => x.ObjectType == ComparisonObjectType.Undefined));
            }
        }
    }
}
