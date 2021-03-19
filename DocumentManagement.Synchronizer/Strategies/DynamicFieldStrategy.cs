using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

        protected override IIncludableQueryable<DynamicField, DynamicField> Include(IQueryable<DynamicField> set)
        {
            return base.Include(set.Include(x => x.ParentField));
        }

        protected override IEnumerable<DynamicField> Order(IEnumerable<DynamicField> list)
            => list.OrderByParent(x => x.ParentField);

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent);

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Add Dynamic Field To Local");

                return await base.AddToLocal(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                tuple.Remote.ExternalID = tuple.Local.ExternalID;

                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent);

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Add Dynamic Field To Remote");

                return await base.AddToRemote(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent);

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Merge Dynamic Field");

                return await base.Merge(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected override async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent);

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Remove Dynamic Field From Local");

                return await base.RemoveFromLocal(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected override async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent);

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Remove Dynamic Field From Remote");

                return await base.RemoveFromRemote(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        protected override DbSet<DynamicField> GetDBSet(DMContext context)
            => context.DynamicFields;

        private async Task<List<SynchronizingResult>> SynchronizeChildren(
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
                var results = await subStrategy.Synchronize(
                    data,
                    connectionContext,
                    tuple.Remote?.ChildrenDynamicFields?.ToList() ?? new List<DynamicField>(),
                    field => field.ParentFieldID == id1 || field.ParentFieldID == id2 ||
                        (field.SynchronizationMate != null &&
                            (field.SynchronizationMate.ParentFieldID == id1 ||
                                field.SynchronizationMate.ParentFieldID == id2)),
                    null,
                    tuple);
                SynchronizeChanges(parent as ISynchronizationChanges, tuple);
                return results;
            }

            return null;
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
