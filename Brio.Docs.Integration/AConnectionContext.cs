using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Threading;

namespace Brio.Docs.Connection
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
