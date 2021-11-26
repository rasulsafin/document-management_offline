using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.LementPro.Synchronization
{
    // TODO: capture remote state and work with it.
    public class LementProConnectionContext : AConnectionContext
    {
        private readonly IFactory<LementProConnectionContext, LementProProjectsSynchronizer> projectSynchronizerFactory;
        private readonly IFactory<
            LementProConnectionContext,
            LementProObjectivesSynchronizer> objectiveSynchronizerFactory;

        public LementProConnectionContext(
            IFactory<LementProConnectionContext, LementProProjectsSynchronizer> projectSynchronizerFactory,
            IFactory<LementProConnectionContext, LementProObjectivesSynchronizer> objectiveSynchronizerFactory)
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
