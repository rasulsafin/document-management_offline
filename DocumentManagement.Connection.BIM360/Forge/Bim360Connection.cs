﻿using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.BIM360.Forge.Services;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.BIM360.Forge
{
    public class Bim360Connection : IConnection
    {
        private AuthenticationService authService;

        public async Task<ConnectionStatusDto> Connect(dynamic param)
        {
            return await authService.SignInAsync((RemoteConnectionInfoDto)param);
        }

        public async Task<ProgressSync> GetProgressSyncronization()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsAuthDataCorrect()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartSyncronization()
        {
            throw new NotImplementedException();
        }

        public async Task StopSyncronization()
        {
            throw new NotImplementedException();
        }
    }
}
