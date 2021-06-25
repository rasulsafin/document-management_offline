using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ItemStrategy<TLinker> : ALinkingStrategy<Item, ItemExternalDto>
        where TLinker : ILinker<Item>
    {
        public ItemStrategy(DMContext context, IMapper mapper, TLinker linker)
            : base(context, mapper, linker)
        {
        }

        public static void UpdateExternalIDs(IEnumerable<Item> local, ICollection<Item> remote)
        {
            foreach (var item in local.Where(x => string.IsNullOrWhiteSpace(x.ExternalID)))
                item.ExternalID = remote.FirstOrDefault(x => x.RelativePath == item.RelativePath)?.ExternalID;
        }

        public async Task FindAndAttachExists(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            object parent,
            string path)
        {
            (int project, int objective) GetParents(bool local)
            {
                var i = 0;
                var objective1 = 0;

                switch (parent)
                {
                    case SynchronizingTuple<Objective> objectiveTuple:
                        var obj = local ? objectiveTuple.Local : objectiveTuple.Synchronized;
                        i = obj.ProjectID;
                        objective1 = obj.ID;
                        break;
                    case SynchronizingTuple<Project> projectTuple:
                        i = (local ? projectTuple.Local : projectTuple.Synchronized).ID;
                        break;
                }

                return (i, objective1);
            }

            var external = tuple.ExternalID;
            int project = 0;
            int objective = 0;

            Expression<Func<Item, bool>> predicate =
                x => ((x.Objectives != null &&
                            x.Objectives.Any(oi => oi.ObjectiveID == objective || oi.Objective.ProjectID == project)) ||
                        x.ProjectID == project) &&
                    ((x.ExternalID != null && x.ExternalID == external) || x.RelativePath == path);

            if ((tuple.Local?.ID ?? 0) == 0)
            {
                (project, objective) = GetParents(true);
                tuple.Local = await context.Items.FirstOrDefaultAsync(predicate);
                tuple.LocalChanged = true;
            }

            if ((tuple.Synchronized?.ID ?? 0) == 0)
            {
                (project, objective) = GetParents(false);
                tuple.Synchronized = await context.Items.FirstOrDefaultAsync(predicate);
                tuple.SynchronizedChanged = true;
            }

            var synced = tuple.Synchronized;

            if (synced != null && tuple.Remote == null)
            {
                tuple.Remote = new Item
                {
                    ExternalID = synced.ExternalID,
                    ItemType = synced.ItemType,
                    RelativePath = synced.RelativePath,
                };
                tuple.RemoteChanged = true;
            }
        }

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            await FindAndAttachExists(tuple, data, parent, tuple.Remote.RelativePath);
            LinkParent(tuple, parent);
            return await base.AddToLocal(tuple, data, connectionContext, parent, token);
        }

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            await FindAndAttachExists(tuple, data, parent, tuple.Local.RelativePath);
            LinkParent(tuple, parent);
            return await base.AddToRemote(tuple, data, connectionContext, parent, token);
        }

        protected override DbSet<Item> GetDBSet(DMContext context)
            => context.Items;

        protected override IIncludableQueryable<Item, Item> Include(IQueryable<Item> set)
            => base.Include(
                set.Include(x => x.Objectives)
                   .Include(x => x.Project));

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            tuple.Local.ExternalID = tuple.Synchronized.ExternalID = tuple.Remote?.ExternalID ?? tuple.ExternalID;
            LinkParent(tuple, parent);
            await NothingAction(tuple, data, connectionContext, parent);
            return null;
        }

        protected override bool IsEntitiesEquals(Item element, SynchronizingTuple<Item> tuple)
            => base.IsEntitiesEquals(element, tuple) ||
                element.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath));

        private static void LinkParent(SynchronizingTuple<Item> tuple, object parent)
        {
            tuple.Remote ??= new Item();
            tuple.Remote.Objectives ??= new List<ObjectiveItem>();

            switch (parent)
            {
                case SynchronizingTuple<Objective> objectiveTuple
                    when tuple.Remote.Objectives.All(x => x.Objective != objectiveTuple.Remote):
                    tuple.Remote.Objectives.Add(new ObjectiveItem { Objective = objectiveTuple.Remote });
                    break;
                case SynchronizingTuple<Project> projectTuple
                    when tuple.Remote.ProjectID == null && tuple.Remote.Project == null:
                    tuple.Remote.Project = projectTuple.Synchronized ?? projectTuple.Local;
                    break;
            }
        }
    }
}
