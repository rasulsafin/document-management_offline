using System;
using System.Collections;
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

namespace MRS.DocumentManagement.Synchronization
{
    public class Synchronizer
    {
        private readonly IMapper mapper;

        public Synchronizer(IMapper mapper)
            => this.mapper = mapper;

        public async Task<SynchronizingResult> Synchronize(
                SynchronizingData data,
                IConnection connection,
                ConnectionInfoDto info)
        {
            var date = DateTime.UtcNow;
            var lastSynchronization = await data.Context.Synchronizations.OrderBy(x => x.Date).LastOrDefaultAsync();
            var context = await connection.GetContext(info, lastSynchronization?.Date ?? DateTime.MinValue);
            var project = new ProjectStrategy(mapper);
            var objective = new ObjectiveStrategy(mapper);
            await project.Synchronize(data, context, mapper.Map<IReadOnlyCollection<Project>>(await context.Projects));
            await objective.Synchronize(
                data,
                context,
                mapper.Map<IReadOnlyCollection<Objective>>(await context.Objectives));
            await data.Context.Synchronizations.AddAsync(new Database.Models.Synchronization { Date = date });
            await data.Context.SaveChangesAsync();
            return null;
        }
    }
}
