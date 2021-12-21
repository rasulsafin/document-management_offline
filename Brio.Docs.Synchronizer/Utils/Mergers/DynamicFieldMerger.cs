using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Mergers
{
    internal class DynamicFieldMerger : IMerger<DynamicField>
    {
        private readonly DMContext context;
        private readonly ILogger<DynamicFieldMerger> logger;

        public DynamicFieldMerger(ILogger<DynamicFieldMerger> logger, DMContext context)
        {
            this.logger = logger;
            this.context = context;
            logger.LogTrace("DynamicFieldMerger created");
        }

        public async Task Merge(SynchronizingTuple<DynamicField> tuple)
        {
            logger.LogTrace(
                "Merge started for the tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local.ID,
                tuple.Synchronized.ID,
                tuple.Remote.ExternalID);
            tuple.Merge(
                field => field.Type,
                field => field.Name,
                field => field.Value,
                x => x.ConnectionInfo,
                x => x.ConnectionInfoID);
            logger.LogDebug("Tuple merged: {@Result}", tuple);
            await LoadChildren(tuple.Local).ConfigureAwait(false);
            await LoadChildren(tuple.Synchronized).ConfigureAwait(false);
            logger.LogTrace("Children loaded");
            if (HasChildren(tuple.Local) || HasChildren(tuple.Synchronized) || HasChildren(tuple.Remote))
                await MergeChildren(tuple).ConfigureAwait(false);
        }

        private bool AddChild(DynamicField parent, DynamicField child)
        {
            logger.LogTrace(
                "AddChild {Child}({ChildId}) started for {Parent}({ParentId})",
                parent.Name,
                parent.ID,
                child.Name,
                child.ID);

            if (!TryGetChild(parent, child, out _))
            {
                parent.ChildrenDynamicFields.Add(child);
                logger.LogDebug("Child added");
                return true;
            }

            logger.LogDebug("Child has already been added");
            return false;
        }

        private bool HasChildren(DynamicField field)
            => (field.ChildrenDynamicFields?.Count ?? 0) > 0;

        private bool IsEntitiesEquals(DynamicField element, SynchronizingTuple<DynamicField> tuple)
        {
            var hasID = element.ID > 0;
            var isLocalsMate = tuple.Local != null && element.ID == tuple.Local.SynchronizationMateID;
            var isSynchronizedsMate = tuple.Synchronized != null &&
                element.SynchronizationMateID == tuple.Synchronized.ID;
            var externalIDEquals = element.ExternalID != null && element.ExternalID == tuple.ExternalID;
            return (hasID && (isLocalsMate || isSynchronizedsMate)) || externalIDEquals;
        }

        private async Task LoadChildren(DynamicField field)
        {
            logger.LogTrace("LoadChildren started for {Object}({ObjectId})", field.Name, field.ID);

            if (field.ChildrenDynamicFields == null)
            {
                if (field.ID != 0)
                {
                    await context.Entry(field)
                       .Collection(x => x.ChildrenDynamicFields)
                       .LoadAsync()
                       .ConfigureAwait(false);
                    logger.LogDebug("Collection of children loaded");
                }
            }
        }

        private async Task MergeChildren(SynchronizingTuple<DynamicField> tuple)
        {
            logger.LogTrace(
                "MergeChildren started for the tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local.ID,
                tuple.Synchronized.ID,
                tuple.Remote.ExternalID);
            tuple.Local.ChildrenDynamicFields ??= new List<DynamicField>();
            tuple.Synchronized.ChildrenDynamicFields ??= new List<DynamicField>();
            tuple.Remote.ChildrenDynamicFields ??= new List<DynamicField>();

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                tuple.Local.ChildrenDynamicFields,
                tuple.Synchronized.ChildrenDynamicFields,
                tuple.Remote.ChildrenDynamicFields,
                IsEntitiesEquals);

            logger.LogDebug("Created {Count}", tuples.Count);

            foreach (var childTuple in tuples)
            {
                var action = childTuple.DetermineAction();
                await Merge(childTuple).ConfigureAwait(false);
                logger.LogDebug("Need {Action} action", action);

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        break;
                    case SynchronizingAction.Merge:
                        break;
                    case SynchronizingAction.AddToLocal:
                        tuple.LocalChanged |= AddChild(tuple.Local, childTuple.Local);
                        tuple.SynchronizedChanged |= AddChild(tuple.Synchronized, childTuple.Synchronized);
                        break;
                    case SynchronizingAction.AddToRemote:
                        tuple.SynchronizedChanged |= AddChild(tuple.Synchronized, childTuple.Synchronized);
                        tuple.RemoteChanged |= AddChild(tuple.Remote, childTuple.Remote);
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        tuple.LocalChanged |=
                            await RemoveChild(tuple.Local, childTuple.Local).ConfigureAwait(false);
                        tuple.SynchronizedChanged |= await RemoveChild(tuple.Synchronized, childTuple.Synchronized)
                           .ConfigureAwait(false);
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        tuple.SynchronizedChanged |= await RemoveChild(tuple.Synchronized, childTuple.Synchronized)
                           .ConfigureAwait(false);
                        tuple.RemoteChanged |= UnlinkChild(tuple.Remote, childTuple.Remote);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), "Incorrect action");
                }

                logger.LogDebug(
                    "Local changed: {Local}, Synchronized changed:{Synchronized}, Remote changed: {Remote}",
                    tuple.LocalChanged,
                    tuple.SynchronizedChanged,
                    tuple.RemoteChanged);
                tuple.SynchronizeChanges(childTuple);
            }
        }

        private async Task<bool> RemoveChild(DynamicField parent, DynamicField child)
        {
            logger.LogTrace(
                "RemoveChild {Child}({ChildId}) started for {Parent}({ParentId})",
                parent.Name,
                parent.ID,
                child.Name,
                child.ID);
            var result = UnlinkChild(parent, child);

            if (await context.DynamicFields.ContainsAsync(child).ConfigureAwait(false))
                context.DynamicFields.Remove(child);

            return result;
        }

        private bool TryGetChild(DynamicField parent, DynamicField child, out DynamicField found)
        {
            found = parent.ChildrenDynamicFields.FirstOrDefault(x => (x.ID != 0 && x.ID == child.ID) || x == child);
            return found != null;
        }

        private bool UnlinkChild(DynamicField parent, DynamicField child)
        {
            logger.LogTrace(
                "UnlinkChild {Child}({ChildId}) started for {Parent}({ParentId})",
                parent.Name,
                parent.ID,
                child.Name,
                child.ID);

            if (TryGetChild(parent, child, out var found))
            {
                parent.ChildrenDynamicFields.Remove(found);
                logger.LogDebug("Child unlinked");
                return true;
            }

            return false;
        }
    }
}
