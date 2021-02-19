using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro
{
    public class LementProConnection : IConnection, IDisposable
    {
        private readonly HttpConnection connector;
        private readonly AuthenticationService authenticationService;
        private readonly HttpRequestUtility requestUtility;

        private ConnectionInfoDto updatedInfo;

        public LementProConnection()
        {
            connector = new HttpConnection();
            requestUtility = new HttpRequestUtility(connector);
            authenticationService = new AuthenticationService(requestUtility);
        }

        public void Dispose()
        {
            connector.Dispose();
            authenticationService.Dispose();
            requestUtility.Dispose();
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
                    AUTH_NAME_END,
                },
                AppProperties = new Dictionary<string, string>(),
            };

            return type;
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }
    }
}
