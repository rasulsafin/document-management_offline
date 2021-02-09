using System;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge.Services;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection.BIM360.Forge
{
    public class Bim360Connection : IConnection
    {
        private AuthenticationService authService;

        public async Task<ConnectionStatusDto> Connect(dynamic param)
        {
            return await authService.SignInAsync((RemoteConnectionInfoDto)param);
        }

        public async Task GetProgressSyncronization()
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
