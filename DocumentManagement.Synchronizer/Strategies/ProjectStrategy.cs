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
                   .Include(x => x.Users));

        protected override async Task AddToRemote(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
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

        protected override async Task Merge(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.Merge(tuple, data, connectionContext, parent);
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
            var projectID = (int)tuple.GetPropertyValue(nameof(Project.ID));
            await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.ToList() ?? new List<Item>(),
                item => (item.ProjectID.HasValue && item.ProjectID == projectID) ||
                    (item.SynchronizationMate.ProjectID.HasValue && item.SynchronizationMate.ProjectID == projectID),
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

        private Task Link(Item item, object parent, SynchronizingData data)
        {
            var tuple = (SynchronizingTuple<Project>)parent;
            var project = item.IsSynchronized ? tuple.Synchronized : tuple.Local;
            if (project == null)
                throw new ArgumentException();

            item.ProjectID = project.ID;
            return Task.CompletedTask;
        }

        private Task Unlink(Item item, object project, SynchronizingData data)
        {
            item.ProjectID = null;
            if (item.Objectives.Count > 0)
                data.Context.Items.Update(item);
            else
                data.Context.Items.Remove(item);
            return Task.CompletedTask;
        }
    }
}
