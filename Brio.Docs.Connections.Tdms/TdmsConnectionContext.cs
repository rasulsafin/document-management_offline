using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using TDMS;

namespace Brio.Docs.Connections.Tdms
{
    public class TdmsConnectionContext : AConnectionContext
    {
        private readonly TDMSApplication tdms;

        public TdmsConnectionContext(TDMSApplication tdms)
        {
            this.tdms = tdms;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer() 
            => new TdmsObjectivesSynchronizer(tdms);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer() 
            => new TdmsProjectsSynchronizer(tdms);
    }
}
