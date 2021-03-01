using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Forge
{
    public class Bim360ConnectionContext : AConnectionContext
    {
        protected override ISynchronizer<ItemExternalDto> CreateItemsSynchronizer()
        {
            throw new NotImplementedException();
        }

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
