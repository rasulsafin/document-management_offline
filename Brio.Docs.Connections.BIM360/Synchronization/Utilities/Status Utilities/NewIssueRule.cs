using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.StatusUtilities
{
    internal class NewIssueRule : IStatusRule
    {
        public StatusRuleResult Evaluate(IEnumerable<Status> objectiveStatusConverting, Status? objectiveDynamicFieldStatus, Issue existing)
        {
            var result = new StatusRuleResult
            {
                CanUse = false,
            };

            if (existing == null)
            {
                result.Status = objectiveDynamicFieldStatus == Status.Draft ? Status.Draft : Status.Open;
                result.CanUse = true;
            }

            return result;
        }
    }
}
