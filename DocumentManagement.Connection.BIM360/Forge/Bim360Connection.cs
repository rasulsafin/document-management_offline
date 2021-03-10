using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Forge
{
    public class Bim360Connection : IConnection, IDisposable
    {
        private AuthenticationService authenticationService;
        private Authenticator authenticator;
        private ForgeConnection connection;

        private ConnectionInfoDto updatedInfo;

        public Bim360Connection()
        {
            connection = new ForgeConnection();
            authenticationService = new AuthenticationService(connection);
            authenticator = new Authenticator(authenticationService);
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            var authorizationResult = await authenticator.SignInAsync(info);
            var updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatusDto> GetStatus()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsAuthDataCorrect()
            => authenticator.IsLogged;

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
            => Task.FromResult(updatedInfo);

        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = "BIM360",
                AuthFieldNames = new List<string>
                {
                    "token",
                    "refreshtoken",
                    "end",
                },
                AppProperties = new Dictionary<string, string>
                {
                    { "CLIENT_ID", "m5fLEAiDRlW3G7vgnkGGGcg4AABM7hCf" },
                    { "CLIENT_SECRET", "dEGEHfbl9LWmEnd7" },
                    { "RETURN_URL", "http://localhost:8000/oauth/" },
                },
            };

            return type;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
