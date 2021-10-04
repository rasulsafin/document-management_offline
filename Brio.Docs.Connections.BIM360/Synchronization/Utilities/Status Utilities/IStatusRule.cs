using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.StatusUtilities
{
    internal interface IStatusRule
    {
        StatusRuleResult Evaluate(IEnumerable<Status> objectiveStatusConverting, Status? objectiveDynamicFieldStatus, Issue existing);
    }
}
