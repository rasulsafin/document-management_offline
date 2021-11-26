using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.StatusUtilities
{
    internal class CanNotUseDynamicFieldStatusRule : IStatusRule
    {
        public StatusRuleResult Evaluate(
            IEnumerable<Status> objectiveStatusConverting,
            Status? objectiveDynamicFieldStatus,
            Issue existing)
        {
            var result = new StatusRuleResult
            {
                CanUse = !existing.Attributes.PermittedStatuses.Contains(
                    objectiveDynamicFieldStatus ?? Status.Undefined),
            };

            if (result.CanUse)
                result.Status = objectiveStatusConverting.First(x => existing.Attributes.PermittedStatuses.Contains(x));

            return result;
        }
    }
}
