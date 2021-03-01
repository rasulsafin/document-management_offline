﻿using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Syncronizers
{
    public class Bim360ProjectSyncronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectsService service;

        public Bim360ProjectSyncronizer(ForgeConnection forgeConnection)
            => service = new ProjectsService(forgeConnection);

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
