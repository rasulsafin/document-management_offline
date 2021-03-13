using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Strategies;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization
{
    public class Synchronizer
    {
        public async Task<ICollection<SynchronizingResult>> Synchronize(
                SynchronizingData data,
                IConnection connection,
                ConnectionInfoExternalDto info)
        {
            var results = new List<SynchronizingResult>();

            try
            {
                var date = DateTime.UtcNow;
                var userID = data.User.ID;
                var lastSynchronization =
                    (await data.Context.Synchronizations.Where(x => x.UserID == userID)
                       .OrderBy(x => x.Date)
                       .LastOrDefaultAsync())?.Date ??
                    DateTime.MinValue;
                var context = await connection.GetContext(data.Mapper.Map<ConnectionInfoExternalDto>(info));
                var project = new ProjectStrategy(data.Mapper);
                var objective = new ObjectiveStrategy(data.Mapper);
                int[] unsyncProjectsIDs;
                string[] unsyncProjectsExternalIDs;

                try
                {
                    var ids = await GetUpdatedIDs(
                        lastSynchronization,
                        data.Context.Projects,
                        context.ProjectsSynchronizer);
                    results.AddRange(
                        await project.Synchronize(
                            data,
                            context,
                            project.Map(await context.ProjectsSynchronizer.Get(ids)),
                            x => x.ExternalID == null || ids.Contains(x.ExternalID),
                            x => x.ExternalID == null || ids.Contains(x.ExternalID)));
                    unsyncProjectsIDs = results.Where(x => x.ObjectType == ObjectType.Local)
                       .Select(x => x.Object.ID)
                       .ToArray();
                    unsyncProjectsExternalIDs = results.Where(x => x.ObjectType == ObjectType.Remote)
                       .Select(x => x.Object.ExternalID)
                       .ToArray();

                    await data.Context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    results.Add(new SynchronizingResult { Exception = e });
                    return results;
                }

                try
                {
                    var ids = await GetUpdatedIDs(
                        lastSynchronization,
                        data.Context.Objectives,
                        context.ObjectivesSynchronizer);
                    var ids2 = ids.ToArray();
                    var OBJECTIVE_EXTERNAL_DTOS = await context.ObjectivesSynchronizer.Get(ids);
                    results.AddRange(
                        await objective.Synchronize(
                            data,
                            context,
                            objective.Map(OBJECTIVE_EXTERNAL_DTOS),
                            x => (x.ExternalID == null || ids.Contains(x.ExternalID))
                             && !unsyncProjectsIDs.Contains(x.ProjectID)
                             && !unsyncProjectsExternalIDs.Contains(x.Project.ExternalID),
                            x => (x.ExternalID == null || ids.Contains(x.ExternalID))
                             && !unsyncProjectsIDs.Contains(x.ProjectID)
                             && !unsyncProjectsExternalIDs.Contains(x.Project.ExternalID)));
                }
                catch (Exception e)
                {
                    results.Add(new SynchronizingResult { Exception = e });
                    return results;
                }

                await data.Context.Synchronizations.AddAsync(
                    new Database.Models.Synchronization
                    {
                        Date = date,
                        UserID = data.User.ID,
                    });
                await data.Context.SaveChangesAsync();
                await SynchronizationFinalizer.Finalize(data);
                await data.Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                results.Add(new SynchronizingResult { Exception = e });
                return results;
            }

            return results;
        }

        private async Task<string[]> GetUpdatedIDs<TDB, TDto>(DateTime date, IQueryable<TDB> set, ISynchronizer<TDto> synchronizer)
            where TDB : class, ISynchronizable<TDB>
        {
            var remoteUpdated = (await synchronizer.GetUpdatedIDs(date)).ToArray();
            var localUpdated = await set.Where(x => x.UpdatedAt > date)
               .Where(x => x.ExternalID != null)
               .Select(x => x.ExternalID)
               .ToListAsync();
            return remoteUpdated.Union(localUpdated).ToArray();
        }
    }
}
