using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Tdms.Mappers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
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
