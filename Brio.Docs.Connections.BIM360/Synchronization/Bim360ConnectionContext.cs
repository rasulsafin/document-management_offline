using Brio.Docs.General.Utils.Factories;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Brio.Docs.Connections.Bim360.Synchronization
{
    public class Bim360ConnectionContext : AConnectionContext, IDisposable
    {
        private readonly IFactory<ISynchronizer<ProjectExternalDto>> projectSynchronizer;
        private readonly IFactory<ISynchronizer<ObjectiveExternalDto>> objectiveSynchronizer;

        private bool isDisposed = false;

        public Bim360ConnectionContext(
            IFactory<ISynchronizer<ProjectExternalDto>> projectSynchronizer,
            IFactory<ISynchronizer<ObjectiveExternalDto>> objectiveSynchronizer)
        {
            this.projectSynchronizer = projectSynchronizer;
            this.objectiveSynchronizer = objectiveSynchronizer;
        }

        public IServiceScope Scope { get; set; }

        public void Dispose()
        {
            if (isDisposed)
                return;

            GC.SuppressFinalize(this);
            isDisposed = true;
            Scope.Dispose();
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => objectiveSynchronizer.Create();

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => projectSynchronizer.Create();
    }
}
