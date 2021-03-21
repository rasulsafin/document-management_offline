using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ItemStrategy : ALinkingStrategy<Item, ItemExternalDto>
    {
        public ItemStrategy(
            IMapper mapper,
            LinkingFunc link,
            LinkingFunc unlink)
            : base(mapper, link, null, unlink)
        {
        }

        protected override async Task<SynchronizingResult> AddToLocal(SynchronizingTuple<Item> tuple, SynchronizingData data, IConnectionContext connectionContext, object parent)
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
            var path = tuple.Remote.RelativePath;
            (int project, int objective) = GetParents(true);
            Expression<Func<Item, bool>> predicate =
                x => ((x.Objectives != null &&
                            x.Objectives.Any(oi => oi.ObjectiveID == objective || oi.Objective.ProjectID == project)) ||
                        x.ProjectID == project) &&
                    (x.ExternalID == external || x.RelativePath == path);
            var compiledPredicate = predicate.Compile();
            tuple.Local = data.Context.Items.Local.FirstOrDefault(compiledPredicate) ??
                await data.Context.Items.FirstOrDefaultAsync(predicate);
            (project, objective) = GetParents(false);
            tuple.Synchronized = data.Context.Items.Local.FirstOrDefault(compiledPredicate) ??
                await data.Context.Items.FirstOrDefaultAsync(predicate);

            return await base.AddToLocal(tuple, data, connectionContext, parent);
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
            object parent)
        {
            tuple.Remote.Objectives ??= new List<ObjectiveItem>();
            if (parent is SynchronizingTuple<Objective> objectiveTuple)
                tuple.Remote.Objectives.Add(new ObjectiveItem { Objective = objectiveTuple.Remote });
            await NothingAction(tuple, data, connectionContext, parent);
            return null;
        }

        protected override bool IsEntitiesEquals(Item element, SynchronizingTuple<Item> tuple)
            => base.IsEntitiesEquals(element, tuple) ||
                 element.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath));
    }
}
