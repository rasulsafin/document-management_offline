using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Brio.Docs.General.Utils.Extensions;
using Brio.Docs.Synchronization.Extensions;

namespace Brio.Docs.Synchronization
{
    public class Synchronizer
    {
        private readonly DMContext dbContext;
        private readonly IMapper mapper;
        private readonly ISynchronizationStrategy<Project, ProjectExternalDto> projectStrategy;
        private readonly ISynchronizationStrategy<Objective, ObjectiveExternalDto> objectiveStrategy;
        private readonly ILogger<Synchronizer> logger;

        public Synchronizer(
            DMContext dbContext,
            IMapper mapper,
            ISynchronizationStrategy<Project, ProjectExternalDto> projectStrategy,
            ISynchronizationStrategy<Objective, ObjectiveExternalDto> objectiveStrategy,
            ILogger<Synchronizer> logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.projectStrategy = projectStrategy;
            this.objectiveStrategy = objectiveStrategy;
            this.logger = logger;
        }

        public async Task<ICollection<SynchronizingResult>> Synchronize(
                SynchronizingData data,
                IConnection connection,
                ConnectionInfoExternalDto info,
                IProgress<double> progress,
                CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Synchronization started with {@Data}", data);

            var results = new List<SynchronizingResult>();
            var projectProgress = new Progress<double>(v => { progress.Report(v / 2); });
            var objectiveProgress = new Progress<double>(v => { progress.Report((v + 1) / 2); });
            IConnectionContext context = null;

            try
            {
                data.Date = DateTime.UtcNow;
                var userID = data.User.ID;
                var lastSynchronization =
                    (await dbContext.Synchronizations.Where(x => x.UserID == userID)
                       .OrderBy(x => x.Date)
                       .LastOrDefaultAsync(CancellationToken.None))?.Date ??
                    DateTime.MinValue;
                context = await connection.GetContext(mapper.Map<ConnectionInfoExternalDto>(info));

                var ids = await GetUpdatedIDs(
                    lastSynchronization,
                    dbContext.Projects.Where(data.ProjectsFilter),
                    context.ProjectsSynchronizer);
                logger.LogDebug("Updated project ids: {@IDs}", (object)ids);
                results.AddRange(
                    await projectStrategy.Synchronize(
                        data,
                        context,
                        projectStrategy.Map(await context.ProjectsSynchronizer.Get(ids)),
                        token,
                        x => x.ExternalID == null || ids.Contains(x.ExternalID),
                        x => x.ExternalID == null || ids.Contains(x.ExternalID),
                        progress: projectProgress));
                logger.LogInformation("Projects synchronized");
                var unsyncProjectsIDs = results.Where(x => x.ObjectType == ObjectType.Local)
                   .Select(x => x.Object.ID)
                   .ToArray();
                logger.LogDebug("Unsynchronized projects: {@IDs}", unsyncProjectsIDs);
                var unsyncProjectsExternalIDs = results.Where(x => x.ObjectType == ObjectType.Remote)
                   .Select(x => x.Object.ExternalID)
                   .ToArray();
                logger.LogDebug("Unsynchronized projects: {@IDs}", (object)unsyncProjectsExternalIDs);

                token.ThrowIfCancellationRequested();

                ids = await GetUpdatedIDs(
                    lastSynchronization,
                    dbContext.Objectives.Where(data.ObjectivesFilter),
                    context.ObjectivesSynchronizer);
                logger.LogDebug("Updated objective ids: {@IDs}", (object)ids);
                results.AddRange(
                    await objectiveStrategy.Synchronize(
                        data,
                        context,
                        objectiveStrategy.Map(await context.ObjectivesSynchronizer.Get(ids)),
                        token,
                        x => (x.ExternalID == null || ids.Contains(x.ExternalID))
                         && !unsyncProjectsIDs.Contains(x.ProjectID)
                         && !unsyncProjectsExternalIDs.Contains(x.Project.ExternalID),
                        x => (x.ExternalID == null || ids.Contains(x.ExternalID))
                         && !unsyncProjectsIDs.Contains(x.ProjectID)
                         && !unsyncProjectsExternalIDs.Contains(x.Project.ExternalID),
                        progress: objectiveProgress));
                logger.LogInformation("Objective synchronized");

                token.ThrowIfCancellationRequested();

                await dbContext.Synchronizations.AddAsync(
                    new Database.Models.Synchronization
                    {
                        Date = data.Date,
                        UserID = data.User.ID,
                    },
                    CancellationToken.None);
                logger.LogTrace("Added synchronization date");
                await dbContext.SynchronizationSaveAsync(data.Date, CancellationToken.None);
                logger.LogDebug("DB updated");
                await SynchronizationFinalizer.Finalize(dbContext);
                logger.LogTrace("Synchronization finalized");
                await dbContext.SynchronizationSaveAsync(data.Date, CancellationToken.None);
                logger.LogDebug("DB updated");
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                return results;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Synchronization failed");
                results.Add(new SynchronizingResult { Exception = e });
                progress?.Report(1.0);
                return results;
            }
            finally
            {
                if (context is IDisposable disposable)
                    disposable.Dispose();
                logger.LogTrace("Context closed");
            }

            return results;
        }

        private async Task<string[]> GetUpdatedIDs<TDB, TDto>(DateTime date, IQueryable<TDB> set, ISynchronizer<TDto> synchronizer)
            where TDB : class, ISynchronizable<TDB>
        {
            logger.LogTrace("GetUpdatedIDs started with date: {@Date}", date);

            // TODO: GetAllIDs to know what is removed from remote.
            var remoteUpdated = (await synchronizer.GetUpdatedIDs(date)).ToArray();
            logger.LogDebug("Updated on remote: {@IDs}", (object)remoteUpdated);
            var localUpdated = await set.Where(x => x.UpdatedAt > date)
               .Where(x => x.ExternalID != null)
               .Select(x => x.ExternalID)
               .ToListAsync();
            logger.LogDebug("Updated on local: {@IDs}", localUpdated);
            var localRemoved = await set
               .Where(x => x.ExternalID != null)
               .GroupBy(x => x.ExternalID)
               .Where(x => x.Count() < 2)
               .Select(x => x.Key)
               .ToListAsync();
            logger.LogDebug("Removed on local: {@IDs}", localRemoved);
            return remoteUpdated.Union(localUpdated).Union(localRemoved).ToArray();
        }
    }
}
