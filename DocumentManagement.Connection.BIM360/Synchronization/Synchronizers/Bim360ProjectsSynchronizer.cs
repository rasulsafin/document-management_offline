using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    public class Bim360ProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectsService service;
        private readonly Bim360ConnectionContext context;

        public Bim360ProjectsSynchronizer(ForgeConnection forgeConnection, Bim360ConnectionContext context)
        {
            service = new ProjectsService(forgeConnection);
            this.context = context;
        }

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectExternalDto> Update(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }
    }
}
