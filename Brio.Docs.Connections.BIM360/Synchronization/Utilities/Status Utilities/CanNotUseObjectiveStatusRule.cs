using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.StatusUtilities
{
    internal class CanNotUseObjectiveStatusRule : IStatusRule
    {
        public StatusRuleResult Evaluate(
            IEnumerable<Status> objectiveStatusConverting,
            Status? objectiveDynamicFieldStatus,
            Issue existing)
        {
            var result = new StatusRuleResult
            {
                CanUse = objectiveStatusConverting.All(x => !existing.Attributes.PermittedStatuses.Contains(x)),
                Status = objectiveDynamicFieldStatus!.Value,
            };
            return result;
        }
    }
}
