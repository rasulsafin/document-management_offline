using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Mergers
{
    internal class DynamicFieldMerger : IMerger<DynamicField>
    {
        private readonly IChildrenMerger<DynamicField, DynamicField> childrenHelper;
        private readonly ILogger<DynamicFieldMerger> logger;

        public DynamicFieldMerger(
            ILogger<DynamicFieldMerger> logger,
            IChildrenMerger<DynamicField, DynamicField> childrenHelper)
        {
            this.logger = logger;
            this.childrenHelper = childrenHelper;
            logger.LogTrace("DynamicFieldMerger created");
        }

        public async Task Merge(SynchronizingTuple<DynamicField> tuple)
        {
            logger.LogTrace(
                "Merge started for the tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.Remote?.ExternalID);
            tuple.Merge(
                field => field.Type,
                field => field.Name,
                field => field.Value,
                x => x.ConnectionInfo,
                x => x.ConnectionInfoID);
            logger.LogDebug("Tuple merged: {@Result}", tuple);
            await childrenHelper.MergeChildren(tuple).ConfigureAwait(false);
        }
    }
}
