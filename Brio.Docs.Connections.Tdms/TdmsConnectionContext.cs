using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Tdms.Mappers;
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
