using MRS.DocumentManagement.Connection.LementPro.Synchronization.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    // TODO: capture remote state and work with it.
    public class LementProConnectionContext : AConnectionContext
    {
        private readonly ProjectSynchronizerFactory projectSynchronizerFactory;
        private readonly ObjectiveSynchronizerFactory objectiveSynchronizerFactory;

        public LementProConnectionContext(
            ProjectSynchronizerFactory projectSynchronizerFactory,
            ObjectiveSynchronizerFactory objectiveSynchronizerFactory)
        {
            this.projectSynchronizerFactory = projectSynchronizerFactory;
            this.objectiveSynchronizerFactory = objectiveSynchronizerFactory;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => objectiveSynchronizerFactory.Create(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => projectSynchronizerFactory.Create(this);
    }
}
