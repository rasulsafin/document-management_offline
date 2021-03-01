using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    public abstract class AConnectionContext
    {
        protected List<ProjectExternalDto> projects;
        protected List<ObjectiveExternalDto> objectives;

        private readonly Lazy<ISynchronizer<ProjectExternalDto>> projectsSynchronizer;
        private readonly Lazy<ISynchronizer<ObjectiveExternalDto>> objectivesSynchronizer;
        private readonly Lazy<ISynchronizer<ItemExternalDto>> itemsSynchronizer;

        protected AConnectionContext()
        {
            projectsSynchronizer = new Lazy<ISynchronizer<ProjectExternalDto>>(
                    CreateProjectsSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            objectivesSynchronizer = new Lazy<ISynchronizer<ObjectiveExternalDto>>(
                    CreateObjectivesSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            itemsSynchronizer = new Lazy<ISynchronizer<ItemExternalDto>>(
                    CreateItemsSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Task<IReadOnlyCollection<ProjectExternalDto>> Projects
            => projects != null ? Task.FromResult((IReadOnlyCollection<ProjectExternalDto>)projects) : GetProjects();

        public Task<IReadOnlyCollection<ObjectiveExternalDto>> Objectives
            => objectives != null
                    ? Task.FromResult((IReadOnlyCollection<ObjectiveExternalDto>)objectives)
                    : GetObjectives();

        public ISynchronizer<ProjectExternalDto> ProjectsSynchronizer
            => projectsSynchronizer.Value;

        public ISynchronizer<ObjectiveExternalDto> ObjectivesSynchronizer
            => objectivesSynchronizer.Value;

        public ISynchronizer<ItemExternalDto> ItemsSynchronizer
            => itemsSynchronizer.Value;

        protected abstract Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects();

        protected abstract Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives();

        protected abstract ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer();

        protected abstract ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer();

        protected abstract ISynchronizer<ItemExternalDto> CreateItemsSynchronizer();
    }
}
