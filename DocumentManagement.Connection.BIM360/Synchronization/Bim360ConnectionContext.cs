using System;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
{
    internal class Bim360ConnectionContext : AConnectionContext, IDisposable
    {
        private readonly IFactory<Bim360ProjectsSynchronizer> projectSynchronizer;
        private readonly IFactory<Bim360ObjectivesSynchronizer> objectiveSynchronizer;

        private bool isDisposed = false;

        public Bim360ConnectionContext(
            IFactory<Bim360ProjectsSynchronizer> projectSynchronizer,
            IFactory<Bim360ObjectivesSynchronizer> objectiveSynchronizer)
        {
            this.projectSynchronizer = projectSynchronizer;
            this.objectiveSynchronizer = objectiveSynchronizer;
        }

        internal IServiceScope Scope { private get; set; }

        internal Bim360Snapshot Snapshot { get; set; } = new Bim360Snapshot();

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
