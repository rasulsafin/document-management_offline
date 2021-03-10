using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
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
            GC.SuppressFinalize(this);
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            var authorizationResult = await authenticationService.SignInAsync(info);
            updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
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

        // Do we need this?
        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        // Do we need this?
        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<IConnectionContext> GetContext(ConnectionInfoDto info, DateTime lastSynchronizationDate)
        {
            throw new NotImplementedException();
        }
    }
}
