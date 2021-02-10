using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.BIM360.Forge.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.BIM360.Forge
{
    public class Bim360Connection : IConnection
    {
        private Authenticator authenticator;

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            return await authenticator.SignInAsync(info);
        }

        public async Task<ProgressSync> GetProgressSyncronization()
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatusDto> GetStatus()
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
