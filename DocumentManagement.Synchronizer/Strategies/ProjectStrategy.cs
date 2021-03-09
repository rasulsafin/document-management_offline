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
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ProjectStrategy : ASynchronizationStrategy<Project, ProjectExternalDto>
    {
        private readonly ItemStrategy itemStrategy;

        public ProjectStrategy(IMapper mapper)
            : base(mapper)
            => itemStrategy = new ItemStrategy(mapper, Link, Unlink);

        protected override DbSet<Project> GetDBSet(DMContext context)
            => context.Projects;

        protected override ISynchronizer<ProjectExternalDto> GetSynchronizer(IConnectionContext context)
            => context.ProjectsSynchronizer;

        protected override Expression<Func<Project, bool>> GetDefaultFilter(SynchronizingData data)
            => data.ProjectsFilter;

        protected override IIncludableQueryable<Project, Project> Include(IQueryable<Project> set)
            => base.Include(
                set
                   .Include(x => x.Users)
                   .Include(x => x.Items));

        protected override async Task AddToRemote(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            tuple.Merge();
            await SynchronizeItems(tuple, data, connectionContext);
            await base.AddToRemote(tuple, data, connectionContext, parent);
        }

        protected override async Task AddToLocal(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await base.AddToLocal(tuple, data, connectionContext, parent);
            await SynchronizeItems(tuple, data, connectionContext);
            AddUser(tuple, data);
        }

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            return await base.Merge(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromLocal(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.RemoveFromLocal(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromRemote(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.RemoveFromRemote(tuple, data, connectionContext, parent);
        }

        private async Task SynchronizeItems(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext)
        {
            var id1 = tuple.Local?.ID ?? 0;
            var id2 = tuple.Synchronized?.ID ?? 0;
            await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.ToList() ?? new List<Item>(),
                item => item.ProjectID == id1 || item.ProjectID == id2 ||
                    (item.SynchronizationMate != null &&
                        (item.SynchronizationMate.ProjectID == id1 || item.SynchronizationMate.ProjectID == id2)),
                tuple);
        }

        private void AddUser(SynchronizingTuple<Project> tuple, SynchronizingData data)
        {
            var projectsFromDB = data.Context.Projects.Include(x => x.Users)
               .Where(x => x.ExternalID == tuple.ExternalID);

            foreach (var project in projectsFromDB)
            {
                project.Users ??= new List<UserProject>();

                if (project.Users.Any(x => x.UserID == data.User.ID))
                    continue;

                project.Users.Add(
                    new UserProject
                    {
                        ProjectID = project.ID,
                        UserID = data.User.ID,
                    });
                data.Context.Projects.Update(project);
            }
        }

        private Task Link(DMContext context, Item item, object parent, EntityType entityType)
        {
            var project = ItemsUtils.GetLinked<Project>(item, parent, entityType);
            project.Items ??= new List<Item>();
            project.Items.Add(item);
            return Task.CompletedTask;
        }

        private Task Unlink(DMContext context, Item item, object parent, EntityType entityType)
        {
            var project = ItemsUtils.GetLinked<Project>(item, parent, entityType);
            item.ProjectID = null;

            if (entityType == EntityType.Remote)
                project.Items.Remove(item);
            else if (item.Objectives?.Count == 0)
                context.Items.Remove(item);
            return Task.CompletedTask;
        }
    }
}
