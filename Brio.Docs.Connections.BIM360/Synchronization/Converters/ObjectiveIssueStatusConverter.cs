using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueStatusConverter : IConverter<ObjectiveExternalDto, Status>
    {
        private readonly Bim360Snapshot snapshot;
        private readonly StatusEnumCreator statusEnumCreator;

        public ObjectiveIssueStatusConverter(Bim360Snapshot snapshot, StatusEnumCreator statusEnumCreator)
        {
            this.snapshot = snapshot;
            this.statusEnumCreator = statusEnumCreator;
        }

        public Task<Status> Convert(ObjectiveExternalDto objective)
        {
            Issue existing = null;
            var project = snapshot.ProjectEnumerable.First(x => x.ID == objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                existing = issueSnapshot.Entity;
            var config = project.StatusesRelations ?? IfcConfigUtilities.GetDefaultStatusesConfig();

            var df = DynamicFieldUtilities.GetValue(
                    statusEnumCreator,
                    project,
                    objective,
                    (ids, statusSnapshot) => ids.Contains(statusSnapshot.Entity.GetEnumMemberValue()),
                    out _)
              ?.Entity;

            if (df != null)
            {
                if (existing == null || CanUse(existing, df.Value) || df.Value != existing.Attributes.Status)
                {
                    var issue = JsonConvert.DeserializeObject<Issue>(
                        JsonConvert.SerializeObject(
                            existing ?? new Issue
                            {
                                Attributes = new Issue.IssueAttributes(),
                            }));
                    issue.Attributes.Status = df.Value;
                    var first = existing.GetSuitableStatuses(config).Append(ObjectiveStatus.Undefined).First();

                    if (first == objective.Status)
                        return Task.FromResult(df.Value);
                }
            }

            if (existing != null &&
                existing.GetSuitableStatuses(config).Append(ObjectiveStatus.Undefined).First() == objective.Status)
                return Task.FromResult(df ?? existing.Attributes.Status);

            var statuses = objective.GetSuitableStatuses(config, existing);

            if (existing != null)
            {
                Status? first = null;

                foreach (var status in statuses)
                {
                    if (CanUse(existing, status))
                    {
                        first ??= status;

                        if (df == null || df == status)
                            return Task.FromResult(status);
                    }
                }

                if (df != null && CanUse(existing, df.Value))
                {
                    if (first == null)
                    {
                        return Task.FromResult(df.Value);
                    }

                    return Task.FromResult(
                        config.Priority.IndexOfFirst(x => x == df) <
                        config.Priority.IndexOfFirst(x => x == first)
                            ? df.Value
                            : first.Value);
                }

                return Task.FromResult(existing.Attributes.Status);
            }

            return Task.FromResult(statuses.Append(Status.Open).First(x => x is Status.Draft or Status.Open));
        }

        private static bool CanUse(Issue existing, Status status)
            => existing.Attributes.PermittedStatuses.Contains(status);
    }
}
