using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.StatusUtilities
{
    internal class CanNotUseStatusesRule : IStatusRule
    {
        public StatusRuleResult Evaluate(
            IEnumerable<Status> objectiveStatusConverting,
            Status? objectiveDynamicFieldStatus,
            Issue existing)
        {
            var result = new StatusRuleResult
            {
                CanUse = objectiveStatusConverting.All(x => !existing.Attributes.PermittedStatuses.Contains(x)),
            };
            result.CanUse &= !existing.Attributes.PermittedStatuses.Contains(objectiveDynamicFieldStatus ?? Status.Undefined);
            result.Status = existing.Attributes.Status;
            return result;
        }
    }
}
