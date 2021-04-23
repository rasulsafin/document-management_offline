using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization
{
    public class Synchronizer
    {
        private readonly DMContext dbContext;
        private readonly IMapper mapper;
        private readonly ISynchronizationStrategy<Project, ProjectExternalDto> projectStrategy;
        private readonly ISynchronizationStrategy<Objective, ObjectiveExternalDto> objectiveStrategy;

        public Synchronizer(
            DMContext dbContext,
            IMapper mapper,
            ISynchronizationStrategy<Project, ProjectExternalDto> projectStrategy,
            ISynchronizationStrategy<Objective, ObjectiveExternalDto> objectiveStrategy)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.projectStrategy = projectStrategy;
            this.objectiveStrategy = objectiveStrategy;
        }

        public async Task<ICollection<SynchronizingResult>> Synchronize(
                SynchronizingData data,
                IConnection connection,
                ConnectionInfoExternalDto info,
                IProgress<double> progress,
                CancellationToken token)
        {
            var results = new List<SynchronizingResult>();
            var projectProgress = new Progress<double>(v => { progress.Report(v / 2); });
            var objectiveProgress = new Progress<double>(v => { progress.Report((v + 1) / 2); });

            try
            {
                data.Date = DateTime.UtcNow;
                var userID = data.User.ID;
                var lastSynchronization =
                    (await dbContext.Synchronizations.Where(x => x.UserID == userID)
                       .OrderBy(x => x.Date)
                       .LastOrDefaultAsync())?.Date ??
                    DateTime.MinValue;
                var context = await connection.GetContext(mapper.Map<ConnectionInfoExternalDto>(info));

                var ids = await GetUpdatedIDs(
                    lastSynchronization,
                    dbContext.Projects,
                    context.ProjectsSynchronizer);
                results.AddRange(
                    await projectStrategy.Synchronize(
                        data,
                        context,
                        projectStrategy.Map(await context.ProjectsSynchronizer.Get(ids)),
                        token,
                        x => x.ExternalID == null || ids.Contains(x.ExternalID),
                        x => x.ExternalID == null || ids.Contains(x.ExternalID),
                        progress: projectProgress));
                var unsyncProjectsIDs = results.Where(x => x.ObjectType == ObjectType.Local)
                   .Select(x => x.Object.ID)
                   .ToArray();
                var unsyncProjectsExternalIDs = results.Where(x => x.ObjectType == ObjectType.Remote)
                   .Select(x => x.Object.ExternalID)
                   .ToArray();

                token.ThrowIfCancellationRequested();

                ids = await GetUpdatedIDs(
                    lastSynchronization,
                    dbContext.Objectives,
                    context.ObjectivesSynchronizer);
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

                token.ThrowIfCancellationRequested();

                await dbContext.Synchronizations.AddAsync(
                    new Database.Models.Synchronization
                    {
                        Date = data.Date,
                        UserID = data.User.ID,
                    });
                await dbContext.SynchronizationSaveAsync(data.Date);
                await SynchronizationFinalizer.Finalize(dbContext);
                await dbContext.SynchronizationSaveAsync(data.Date);
            }
            catch (OperationCanceledException)
            {
                return results;
            }
            catch (Exception e)
            {
                results.Add(new SynchronizingResult { Exception = e });
                progress?.Report(1.0);
                return results;
            }

            return results;
        }

        private async Task<string[]> GetUpdatedIDs<TDB, TDto>(DateTime date, IQueryable<TDB> set, ISynchronizer<TDto> synchronizer)
            where TDB : class, ISynchronizable<TDB>
        {
            // TODO: GetAllIDs to know what is removed from remote.
            var remoteUpdated = (await synchronizer.GetUpdatedIDs(date)).ToArray();
            var localUpdated = await set.Where(x => x.UpdatedAt > date)
               .Where(x => x.ExternalID != null)
               .Select(x => x.ExternalID)
               .ToListAsync();
            var localRemoved = await set
               .Where(x => x.ExternalID != null)
               .GroupBy(x => x.ExternalID)
               .Where(x => x.Count() < 2)
               .Select(x => x.Key)
               .ToListAsync();
            return remoteUpdated.Union(localUpdated).Union(localRemoved).ToArray();
        }
    }
}
