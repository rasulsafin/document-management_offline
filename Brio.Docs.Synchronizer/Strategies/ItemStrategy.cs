using System;
using System.Collections.Generic;
using System.Linq;
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
    [Obsolete]
    internal class ItemStrategy<TLinker> : ALinkingStrategy<Item, ItemExternalDto>
        where TLinker : ILinker<Item>
    {
        private readonly IMerger<Item> merger;
        private readonly IFinder<Item> itemFinder;
        private readonly ILogger<ItemStrategy<TLinker>> logger;

        public ItemStrategy(
            IMerger<Item> merger,
            DMContext context,
            IMapper mapper,
            TLinker linker,
            IFinder<Item> itemFinder,
            ILogger<ItemStrategy<TLinker>> logger)
            : base(context, mapper, linker, logger)
        {
            this.merger = merger;
            this.itemFinder = itemFinder;
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
            object parent)
        {
            logger.LogStartAction(tuple, data, parent);

            if (tuple.Remote != null)
            {
                if (tuple.Remote.ProjectID == null && tuple.Remote.Objectives == null)
                {
                    switch (parent)
                    {
                        case SynchronizingTuple<Project> projects:
                            tuple.Remote.ProjectID = (int)projects.GetPropertyValue(nameof(Project.ID));
                            break;
                        case SynchronizingTuple<Objective> objectives:
                            tuple.Remote.Objectives = new List<ObjectiveItem>
                            {
                                new ()
                                {
                                    Objective = objectives.Remote ?? objectives.Synchronized ?? objectives.Local,
                                },
                            };
                            break;
                    }
                }
            }

            await itemFinder.AttachExisting(tuple);
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

            await FindAndAttachExists(tuple, data, parent);
            logger.LogTrace("Attached");
            LinkParent(tuple, parent);
            logger.LogTrace("Parent linked");
            merger.Merge(tuple);
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

            await FindAndAttachExists(tuple, data, parent);
            logger.LogTrace("Attached");
            LinkParent(tuple, parent);
            logger.LogTrace("Parent linked");
            merger.Merge(tuple);
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
            merger.Merge(tuple);
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
