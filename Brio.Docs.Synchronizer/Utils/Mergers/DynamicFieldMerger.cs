using System;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Mergers
{
    internal class DynamicFieldMerger : IMerger<DynamicField>
    {
        private readonly Lazy<IChildrenMerger<DynamicField, DynamicField>> childrenHelper;
        private readonly ILogger<DynamicFieldMerger> logger;

        public DynamicFieldMerger(
            ILogger<DynamicFieldMerger> logger,
            IFactory<IChildrenMerger<DynamicField, DynamicField>> childrenHelperFactory)
        {
            this.logger = logger;
            this.childrenHelper = new Lazy<IChildrenMerger<DynamicField, DynamicField>>(childrenHelperFactory.Create);
            logger.LogTrace("DynamicFieldMerger created");
        }

        public async ValueTask Merge(SynchronizingTuple<DynamicField> tuple)
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
            await childrenHelper.Value.MergeChildren(tuple).ConfigureAwait(false);
        }
    }
}
