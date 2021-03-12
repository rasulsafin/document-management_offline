using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Strategies;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization
{
    public class Synchronizer
    {
        private readonly IMapper mapper;

        public Synchronizer(IMapper mapper)
            => this.mapper = mapper;

        public async Task<ICollection<SynchronizingResult>> Synchronize(
                SynchronizingData data,
                IConnection connection,
                ConnectionInfo info)
        {
            var results = new List<SynchronizingResult>();

            try
            {
                var date = DateTime.UtcNow;
                var lastSynchronization =
                    (await data.Context.Synchronizations.OrderBy(x => x.Date).LastOrDefaultAsync())?.Date ??
                    DateTime.MinValue;
                var context = await connection.GetContext(mapper.Map<ConnectionInfoExternalDto>(info));
                var project = new ProjectStrategy(mapper);
                var objective = new ObjectiveStrategy(mapper);
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
                            x => x.ExternalID == null || ids.Contains(x.ExternalID)));
                    unsyncProjectsIDs = results.Where(x => x.ObjectType == ObjectType.Local)
                       .Select(x => x.Object.ID)
                       .ToArray();
                    unsyncProjectsExternalIDs = results.Where(x => x.ObjectType == ObjectType.Remote)
                       .Select(x => x.Object.ExternalID)
                       .ToArray();
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
                    results.AddRange(
                        await objective.Synchronize(
                            data,
                            context,
                            objective.Map(await context.ObjectivesSynchronizer.Get(ids)),
                            x => (x.ExternalID == null || ids.Contains(x.ExternalID))
                             && !unsyncProjectsIDs.Contains(x.ID)
                             && !unsyncProjectsExternalIDs.Contains(x.ExternalID)));
                }
                catch (Exception e)
                {
                    results.Add(new SynchronizingResult { Exception = e });
                    return results;
                }

                await data.Context.Synchronizations.AddAsync(new Database.Models.Synchronization { Date = date });
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
            var remoteUpdated = await synchronizer.GetUpdatedIDs(date);
            var localUpdated = await set.Where(x => x.UpdatedAt > date)
               .Where(x => x.ExternalID != null)
               .Select(x => x.ExternalID)
               .ToListAsync();
            return remoteUpdated.Union(localUpdated).ToArray();
        }
    }
}
