using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronizer.Extensions;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer.Strategies
{
    internal class ProjectStrategy : ASynchronizationStrategy<Project, ProjectExternalDto>
    {
        private readonly ItemStrategy itemStrategy;

        public ProjectStrategy(IMapper mapper, ItemStrategy itemStrategy)
            : base(mapper)
        {
            this.itemStrategy = itemStrategy;
        }

        protected override DbSet<Project> GetDBSet(DMContext context)
            => context.Projects;

        protected override ISynchronizer<ProjectExternalDto> GetSynchronizer(AConnectionContext context)
            => context.ProjectsSynchronizer;

        protected override bool DefaultFilter(SynchronizingData data, Project project)
            => data.ProjectsFilter(project);

        protected override async Task Merge(SynchronizingTuple<Project> tuple, SynchronizingData data, AConnectionContext connectionContext)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.Merge(tuple, data, connectionContext);
        }

        protected override async Task AddToRemote(SynchronizingTuple<Project> tuple, SynchronizingData data, AConnectionContext connectionContext)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.AddToRemote(tuple, data, connectionContext);
        }

        private async Task SynchronizeItems(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            await itemStrategy.Synchronize(
                data,
                connectionContext,
                item => item.ProjectID.HasValue && item.ProjectID == (int) tuple.GetPropertyValue(nameof(Project.ID)));
        }
    }
}
