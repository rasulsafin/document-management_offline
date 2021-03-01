using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Syncronizers
{
    public class Bim360ObjectiveSyncronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;

        public Bim360ObjectiveSyncronizer(ForgeConnection forgeConnection)
        {
            issuesService = new IssuesService(forgeConnection);
            itemsService = new ItemsService(forgeConnection);
        }

        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }
    }
}
