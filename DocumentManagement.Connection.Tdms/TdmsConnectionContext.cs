using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsConnectionContext : AConnectionContext
    {
        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            throw new NotImplementedException();
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            throw new NotImplementedException();
        }

        protected override Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives()
        {
            throw new NotImplementedException();
        }

        protected override Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects()
        {
            throw new NotImplementedException();
        }
    }
}
