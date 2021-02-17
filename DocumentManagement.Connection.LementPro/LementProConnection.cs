using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro
{
    public class LementProConnection : IConnection, IDisposable
    {
        private readonly NetConnector connector;
        private readonly AuthenticationService authenticationService;

        private ConnectionInfoDto updatedInfo;

        public LementProConnection()
        {
            connector = new NetConnector();
            authenticationService = new AuthenticationService(connector);
        }

        public void Dispose()
        {
            connector.Dispose();
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            var authorizationResult = await authenticationService.SignInAsync(info);
            updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
        }

        public async Task<ConnectionStatusDto> GetStatus()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsAuthDataCorrect()
        {
            throw new NotImplementedException();
        }

        public async Task<ProgressSync> GetProgressSyncronization()
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

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
            => Task.FromResult(updatedInfo);

        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = "LementPro",
                AuthFieldNames = new List<string>
                {
                    AUTH_NAME_LOGIN,
                    AUTH_NAME_PASSWORD,
                    AUTH_NAME_TOKEN,
                    // TODO: Check does success login response returns also token expiration date
                    //"end",
                },
                AppProperty = new Dictionary<string, string>(),
            };

            return type;
        }
    }
}
