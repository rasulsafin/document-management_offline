using Brio.Docs.General.Utils.Factories;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Connection.LementPro.Synchronization
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
