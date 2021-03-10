using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class DynamicFieldStrategy : ALinkingStrategy<DynamicField, DynamicFieldExternalDto>
    {
        public DynamicFieldStrategy(IMapper mapper, LinkingFunc link, LinkingFunc update, LinkingFunc unlink)
            : base(mapper, link, update, unlink)
        {
        }

        protected override async Task AddToLocal(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeChildren(tuple, data, connectionContext, parent);
            await base.AddToLocal(tuple, data, connectionContext, parent);
        }

        protected override async Task AddToRemote(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeChildren(tuple, data, connectionContext, parent);
            await base.AddToRemote(tuple, data, connectionContext, parent);
        }

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeChildren(tuple, data, connectionContext, parent);
            return await base.Merge(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromLocal(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeChildren(tuple, data, connectionContext, parent);
            await base.RemoveFromLocal(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromRemote(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeChildren(tuple, data, connectionContext, parent);
            await base.RemoveFromRemote(tuple, data, connectionContext, parent);
        }

        protected override DbSet<DynamicField> GetDBSet(DMContext context)
            => context.DynamicFields;

        private async Task SynchronizeChildren(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            if (HasChildren(tuple.Local) || HasChildren(tuple.Remote) || HasChildren(tuple.Synchronized))
            {
                tuple.Merge();
                var subStrategy = new DynamicFieldStrategy(mapper, Link, Update, Unlink);
                var id1 = tuple.Local?.ID ?? 0;
                var id2 = tuple.Synchronized?.ID ?? 0;
                await subStrategy.Synchronize(
                    data,
                    connectionContext,
                    tuple.Remote?.ChildrenDynamicFields?.ToList() ?? new List<DynamicField>(),
                    field => field.ParentFieldID == id1 || field.ParentFieldID == id2 ||
                        (field.SynchronizationMate != null &&
                            (field.SynchronizationMate.ParentFieldID == id1 ||
                                field.SynchronizationMate.ParentFieldID == id2)),
                    tuple);
                SynchronizeChanges(parent as ISynchronizationChanges, tuple);
            }
        }

        private bool HasChildren(DynamicField field)
            => (field?.ChildrenDynamicFields?.Count ?? 0) != 0;

        private Task Unlink(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            var p =  LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            field.ParentField = null;

            if (entityType == EntityType.Remote)
                p.ChildrenDynamicFields.Remove(field);
            else if (field.ParentFieldID == null)
                context.DynamicFields.Remove(field);
            else
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }

        private Task Update(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            if (entityType != EntityType.Remote)
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }

        private Task Link(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            var p = LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            p.ChildrenDynamicFields ??= new List<DynamicField>();
            p.ChildrenDynamicFields.Add(field);
            return Task.CompletedTask;
        }

        private void SynchronizeChanges(ISynchronizationChanges parentTuple, ISynchronizationChanges childTuple)
        {
            parentTuple.LocalChanged |= childTuple.LocalChanged;
            parentTuple.SynchronizedChanged |= childTuple.SynchronizedChanged;
            parentTuple.RemoteChanged |= childTuple.RemoteChanged;
        }
    }
}
