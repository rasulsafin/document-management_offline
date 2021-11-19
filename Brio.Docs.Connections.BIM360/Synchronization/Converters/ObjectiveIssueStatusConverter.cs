using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities.StatusUtilities;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueStatusConverter : IConverter<ObjectiveExternalDto, Status>
    {
        private readonly Bim360Snapshot snapshot;
        private readonly StatusEnumCreator statusEnumCreator;

        private readonly IEnumerable<IStatusRule> rules = new IStatusRule[]
        {
            new NewIssueRule(),
            new CanNotUseStatusesRule(),
            new CanNotUseDynamicFieldStatusRule(),
            new CanNotUseObjectiveStatusRule(),
            new DynamicFieldNotChangedRule(),
        };

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

            var statuses = objective.GetSuitableStatuses(config, existing).ToArray();

            foreach (var rule in rules)
            {
                var result = rule.Evaluate(statuses, df, existing);
                if (result.CanUse)
                    return Task.FromResult(result.Status);
            }

            return Task.FromResult(Status.Open);
        }
    }
}
