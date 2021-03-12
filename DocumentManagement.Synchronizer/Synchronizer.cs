using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
                var lastSynchronization = await data.Context.Synchronizations.OrderBy(x => x.Date).LastOrDefaultAsync();
                var context = await connection.GetContext(
                    mapper.Map<ConnectionInfoExternalDto>(info),
                    lastSynchronization?.Date ?? DateTime.MinValue);
                var project = new ProjectStrategy(mapper);
                var objective = new ObjectiveStrategy(mapper);

                try
                {
                    results.AddRange(await project.Synchronize(
                        data,
                        context,
                        project.Map(await context.Projects)));
                }
                catch (Exception e)
                {
                    results.Add(
                        new SynchronizingResult()
                        {
                            Exception = e,
                        });

                    return results;
                }

                try
                {
                    results.AddRange(
                        await objective.Synchronize(
                            data,
                            context,
                            objective.Map(await context.Objectives)));
                }
                catch (Exception e)
                {
                    results.Add(
                        new SynchronizingResult()
                        {
                            Exception = e,
                        });

                    return results;
                }

                await data.Context.Synchronizations.AddAsync(new Database.Models.Synchronization { Date = date });
                await data.Context.SaveChangesAsync();
                await SynchronizationFinalizer.Finalize(data);
                await data.Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                results.Add(
                    new SynchronizingResult()
                    {
                        Exception = e,
                    });

                return results;
            }

            return results;
        }
    }
}
