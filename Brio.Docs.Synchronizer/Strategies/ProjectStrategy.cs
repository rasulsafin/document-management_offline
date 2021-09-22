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
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils.Linkers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ProjectStrategy : ASynchronizationStrategy<Project, ProjectExternalDto>
    {
        private readonly ItemStrategy<ProjectItemLinker> itemStrategy;
        private readonly ILogger<ProjectStrategy> logger;

        public ProjectStrategy(
            DMContext context,
            IMapper mapper,
            ItemStrategy<ProjectItemLinker> itemStrategy,
            ILogger<ProjectStrategy> logger)
            : base(context, mapper, logger)
        {
            this.itemStrategy = itemStrategy;
            this.logger = logger;
            logger.LogTrace("ProjectStrategy created");
        }

        protected override DbSet<Project> GetDBSet(DMContext source)
            => source.Projects;

        protected override ISynchronizer<ProjectExternalDto> GetSynchronizer(IConnectionContext source)
            => source.ProjectsSynchronizer;

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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                logger.LogBeforeMerge(tuple);
                tuple.Merge();
                logger.LogAfterMerge(tuple);
                AddUser(tuple, data);
                logger.LogTrace("User linked");
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext, token);
                logger.LogTrace("Items synchronized");
                if (resultAfterItemSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing Items in Add Project To Remote");

                var result = await base.AddToRemote(tuple, data, connectionContext, parent, token);
                UpdateChildrenAfterSynchronization(tuple);
                logger.LogTrace("Children updated");
                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToRemote, e, tuple);
                return new SynchronizingResult
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                logger.LogBeforeMerge(tuple);
                tuple.Merge();
                logger.LogAfterMerge(tuple);
                AddUser(tuple, data);

                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent, token);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext, token);
                logger.LogTrace("Items synchronized");
                if (resultAfterItemSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing Items in Add Project To Local");

                return null;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToLocal, e, tuple);
                return new SynchronizingResult
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                var resultAfterItemSync = await SynchronizeItems(tuple, data, connectionContext, token);
                logger.LogTrace("Items synchronized");
                if (resultAfterItemSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing Items in Merge Project");

                AddUser(tuple, data);
                logger.LogTrace("User linked");

                var result = await base.Merge(tuple, data, connectionContext, parent, token);
                UpdateChildrenAfterSynchronization(tuple);
                logger.LogTrace("Children updated");
                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.Merge, e, tuple);
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                return await base.RemoveFromLocal(tuple, data, connectionContext, parent, token);
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromLocal, e, tuple);
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                return await base.RemoveFromRemote(tuple, data, connectionContext, parent, token);
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromRemote, e, tuple);
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
            logger.LogTrace("SynchronizeItems started with tuple: {@Tuple}, data: {@Data}", tuple, data);
            var id1 = tuple.Local?.ID ?? 0;
            var id2 = tuple.Synchronized?.ID ?? 0;
            logger.LogDebug("Local id = {@Local}, Synchronized id = {@Synchronized}", id1, id2);
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
        {
            logger.LogTrace("UpdateChildrenAfterSynchronization started with tuple: {@Tuple}", tuple);
            ItemStrategy<ProjectItemLinker>.UpdateExternalIDs(
                (tuple.Local.Items ?? ArraySegment<Item>.Empty).Concat(
                    tuple.Synchronized.Items ?? ArraySegment<Item>.Empty),
                tuple.Remote.Items ?? ArraySegment<Item>.Empty);
        }

        private void AddUser(SynchronizingTuple<Project> tuple, SynchronizingData data)
        {
            logger.LogTrace("SynchronizeItems started with tuple: {@Tuple}, data: {@Data}", tuple, data);

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
                    logger.LogDebug("Added user {ID} to project: {@Project}", data.User.ID, project);
                }
            }

            AddUserLocal(tuple.Local);
            logger.LogTrace("Added user to local");
            AddUserLocal(tuple.Synchronized);
            logger.LogTrace("Added user to synchronized");
        }
    }
}
