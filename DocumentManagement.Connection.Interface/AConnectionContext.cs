using System;
using System.Threading;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    public abstract class AConnectionContext : IConnectionContext
    {
        private readonly Lazy<ISynchronizer<ProjectExternalDto>> projectsSynchronizer;
        private readonly Lazy<ISynchronizer<ObjectiveExternalDto>> objectivesSynchronizer;

        protected AConnectionContext()
        {
            projectsSynchronizer = new Lazy<ISynchronizer<ProjectExternalDto>>(
                    CreateProjectsSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            objectivesSynchronizer = new Lazy<ISynchronizer<ObjectiveExternalDto>>(
                    CreateObjectivesSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ISynchronizer<ProjectExternalDto> ProjectsSynchronizer
            => projectsSynchronizer.Value;

        public ISynchronizer<ObjectiveExternalDto> ObjectivesSynchronizer
            => objectivesSynchronizer.Value;

        protected abstract ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer();

        protected abstract ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer();
    }
}
