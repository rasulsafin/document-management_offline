using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connection.Tdms.Mappers;
using TDMS;

namespace Brio.Docs.Connection.Tdms
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
