using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;
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

        private ConnectionInfoExternalDto updatedInfo;

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
            GC.SuppressFinalize(this);
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info)
        {
            var authorizationResult = await authenticationService.SignInAsync(info);
            updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
            => Task.FromResult(updatedInfo);

        public ConnectionTypeExternalDto GetConnectionType()
        {
            var type = new ConnectionTypeExternalDto
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
        public Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }

        // Do we need this?
        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => await LementProConnectionContext.CreateContext(info);

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await Connect(info);
            return new LementProConnectionStorage(requestUtility);
        }
    }
}
