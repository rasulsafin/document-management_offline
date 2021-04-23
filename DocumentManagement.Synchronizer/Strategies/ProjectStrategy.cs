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
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils.Linkers;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ProjectStrategy : ASynchronizationStrategy<Project, ProjectExternalDto>
    {
        private readonly ItemStrategy<ProjectItemLinker> itemStrategy;

        public ProjectStrategy(DMContext context, IMapper mapper, ItemStrategy<ProjectItemLinker> itemStrategy)
            : base(context, mapper)
            => this.itemStrategy = itemStrategy;

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
            object parent,
            CancellationToken token)
        {
            try
            {
                tuple.Merge();
                AddUser(tuple, data);
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext, token);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Add Project To Remote");

                var result = await base.AddToRemote(tuple, data, connectionContext, parent, token);
                UpdateChildrenAfterSynchronization(tuple);
                return result;
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
            object parent,
            CancellationToken token)
        {
            try
            {
                tuple.Merge();
                AddUser(tuple, data);

                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent, token);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext, token);
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
            object parent,
            CancellationToken token)
        {
            try
            {
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext, token);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Merge Project");

                AddUser(tuple, data);

                var result = await base.Merge(tuple, data, connectionContext, parent, token);
                UpdateChildrenAfterSynchronization(tuple);
                return result;
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
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            try
            {
                return await base.RemoveFromLocal(tuple, data, connectionContext, parent, token);
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
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            try
            {
                return await base.RemoveFromRemote(tuple, data, connectionContext, parent, token);
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

        private async Task<List<SynchronizingResult>> SynchronizeItems(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            CancellationToken token)
        {
            var id1 = tuple.Local?.ID ?? 0;
            var id2 = tuple.Synchronized?.ID ?? 0;
            return await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.ToList() ?? new List<Item>(),
                token,
                item => item.ProjectID == id1 || item.ProjectID == id2 ||
                    (item.SynchronizationMate != null &&
                        (item.SynchronizationMate.ProjectID == id1 || item.SynchronizationMate.ProjectID == id2)),
                null,
                tuple);
        }

        private void UpdateChildrenAfterSynchronization(SynchronizingTuple<Project> tuple)
            => ItemStrategy<ProjectItemLinker>.UpdateExternalIDs(
                (tuple.Local.Items ?? ArraySegment<Item>.Empty).Concat(
                    tuple.Synchronized.Items ?? ArraySegment<Item>.Empty),
                tuple.Remote.Items ?? ArraySegment<Item>.Empty);

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
    }
}
