using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronizer.Models;
using MRS.DocumentManagement.Synchronizer.Strategies;

namespace MRS.DocumentManagement.Synchronizer
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
            var lastSynchronization = await data.Context.Synchronizations.LastOrDefaultAsync();
            var context = await connection.GetContext(info, lastSynchronization?.Date ?? DateTime.MinValue);
            var project = new ProjectStrategy(mapper);
            var objective = new ObjectiveStrategy(mapper);
            await project.Synchronize(data, context);
            await objective.Synchronize(data, context);
            await data.Context.Synchronizations.AddAsync(new Synchronization { Date = date });
        }
    }
}
