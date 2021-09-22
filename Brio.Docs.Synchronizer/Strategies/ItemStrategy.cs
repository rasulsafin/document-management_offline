using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ItemStrategy<TLinker> : ALinkingStrategy<Item, ItemExternalDto>
        where TLinker : ILinker<Item>
    {
        private readonly ILogger<ItemStrategy<TLinker>> logger;

        public ItemStrategy(DMContext context, IMapper mapper, TLinker linker, ILogger<ItemStrategy<TLinker>> logger)
            : base(context, mapper, linker, logger)
        {
            this.logger = logger;
            logger.LogTrace("ItemStrategy created");
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
            logger.LogStartAction(tuple, data, parent);

            (int project, int objective) GetParents(bool local)
            {
                var i = 0;
                var objective1 = 0;

                switch (parent)
                {
                    case SynchronizingTuple<Objective> objectiveTuple:
                        logger.LogDebug("Parent is a objective");
                        var obj = local ? objectiveTuple.Local : objectiveTuple.Synchronized;
                        i = obj.ProjectID;
                        objective1 = obj.ID;
                        break;
                    case SynchronizingTuple<Project> projectTuple:
                        logger.LogDebug("Parent is a project");
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
                logger.LogDebug("Local searching");
                (project, objective) = GetParents(true);
                logger.LogDebug("Project {@Project}, Objective {@Objective}", project, objective);
                tuple.Local = await context.Items.FirstOrDefaultAsync(predicate);
                logger.LogDebug("Found item: {@Object}", tuple.Local);
                tuple.LocalChanged = true;
            }

            if ((tuple.Synchronized?.ID ?? 0) == 0)
            {
                logger.LogDebug("Synchronized searching");
                (project, objective) = GetParents(false);
                logger.LogDebug("Project {@Project}, Objective {@Objective}", project, objective);
                tuple.Synchronized = await context.Items.FirstOrDefaultAsync(predicate);
                logger.LogDebug("Found item: {@Object}", tuple.Local);
                tuple.SynchronizedChanged = true;
            }

            var synced = tuple.Synchronized;

            if (synced != null && tuple.Remote == null)
            {
                logger.LogDebug("Creating remote");

                tuple.Remote = new Item
                {
                    ExternalID = synced.ExternalID,
                    ItemType = synced.ItemType,
                    RelativePath = synced.RelativePath,
                    ProjectID = synced.ProjectID,
                };
                logger.LogDebug("Created item: {@Object}", tuple.Local);
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            await FindAndAttachExists(tuple, data, parent, tuple.Remote.RelativePath);
            logger.LogTrace("Attached");
            LinkParent(tuple, parent);
            logger.LogTrace("Parent linked");
            return await base.AddToLocal(tuple, data, connectionContext, parent, token);
        }

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            await FindAndAttachExists(tuple, data, parent, tuple.Local.RelativePath);
            logger.LogTrace("Attached");
            LinkParent(tuple, parent);
            logger.LogTrace("Parent linked");
            return await base.AddToRemote(tuple, data, connectionContext, parent, token);
        }

        protected override DbSet<Item> GetDBSet(DMContext context)
            => context.Items;

        protected override IIncludableQueryable<Item, Item> Include(IQueryable<Item> set)
            => base.Include(
                set.Include(x => x.Objectives)
                   .Include(x => x.Project));

        protected override Task<SynchronizingResult> Merge(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            if (tuple.Synchronized == null)
            {
                tuple.Merge();
                tuple.Synchronized.Project = tuple.Local.Project.SynchronizationMate;
            }

            tuple.Local.ExternalID = tuple.Synchronized.ExternalID = tuple.Remote?.ExternalID ?? tuple.ExternalID;
            LinkParent(tuple, parent);
            logger.LogTrace("Parent linked");
            return Task.FromResult<SynchronizingResult>(null);
        }

        protected override bool IsEntitiesEquals(Item element, SynchronizingTuple<Item> tuple)
            => base.IsEntitiesEquals(element, tuple) ||
                element.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath));

        private void LinkParent(SynchronizingTuple<Item> tuple, object parent)
        {
            logger.LogTrace("LinkParent started with tuple:\r\n{@Tuple}\r\nparent:\r\n{@Parent}", tuple, parent);
            tuple.Remote ??= new Item();
            tuple.Remote.Objectives ??= new List<ObjectiveItem>();

            switch (parent)
            {
                case SynchronizingTuple<Objective> objectiveTuple
                    when tuple.Remote.Objectives.All(x => x.Objective != objectiveTuple.Remote):
                    tuple.Remote.Objectives.Add(new ObjectiveItem { Objective = objectiveTuple.Remote });
                    logger.LogDebug("Added link to objective");
                    break;
                case SynchronizingTuple<Project> projectTuple
                    when tuple.Remote.ProjectID == null && tuple.Remote.Project == null:
                    tuple.Remote.Project = projectTuple.Synchronized ?? projectTuple.Local;
                    logger.LogDebug("Added link to project");
                    break;
            }
        }
    }
}
