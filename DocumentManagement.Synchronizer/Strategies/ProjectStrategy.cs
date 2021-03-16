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

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                AddUser(tuple, data);
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Add Project To Remote");

                return await base.AddToRemote(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                AddUser(tuple, data);

                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Add Project To Local");

                return null;
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Merge Project");

                AddUser(tuple, data);

                return await base.Merge(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected override async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Remove Project From Local");

                return await base.RemoveFromLocal(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected override async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Remove Project From Remote");

                return await base.RemoveFromRemote(tuple, data, connectionContext, parent);
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        private async Task<List<SynchronizingResult>> SynchronizeItems(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext)
        {
            var id1 = tuple.Local?.ID ?? 0;
            var id2 = tuple.Synchronized?.ID ?? 0;
            return await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.ToList() ?? new List<Item>(),
                item => item.ProjectID == id1 || item.ProjectID == id2 ||
                    (item.SynchronizationMate != null &&
                        (item.SynchronizationMate.ProjectID == id1 || item.SynchronizationMate.ProjectID == id2)),
                null,
                tuple);
        }

        private void AddUser(SynchronizingTuple<Project> tuple, SynchronizingData data)
        {
            void AddUserLocal(Project project)
            {
                project.Users ??= new List<UserProject>();

                if (project.Users.All(x => x.UserID != data.User.ID))
                {
                    project.Users.Add(
                        new UserProject
                        {
                            Project = project,
                            UserID = data.User.ID,
                        });
                }
            }

            AddUserLocal(tuple.Local);
            AddUserLocal(tuple.Synchronized);
        }

        private Task Link(DMContext context, Item item, object parent, EntityType entityType)
        {
            var project = LinkingUtils.CheckAndUpdateLinking<Project>(parent, entityType);
            project.Items ??= new List<Item>();
            project.Items.Add(item);
            return Task.CompletedTask;
        }

        private Task Unlink(DMContext context, Item item, object parent, EntityType entityType)
        {
            var project = LinkingUtils.CheckAndUpdateLinking<Project>(parent, entityType);
            item.ProjectID = null;

            if (entityType == EntityType.Remote)
                project.Items.Remove(item);
            else if (item.Objectives?.Count == 0)
                context.Items.Remove(item);
            else
                context.Items.Update(item);
            return Task.CompletedTask;
        }
    }
}
