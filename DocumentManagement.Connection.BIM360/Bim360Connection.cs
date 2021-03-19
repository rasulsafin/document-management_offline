using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public class Bim360Connection : IConnection, IDisposable
    {
        private AuthenticationService authenticationService;
        private Authenticator authenticator;
        private ForgeConnection connection;

        private ConnectionInfoExternalDto updatedInfo;

        public Bim360Connection()
        {
            connection = new ForgeConnection();
            authenticationService = new AuthenticationService(connection);
            authenticator = new Authenticator(authenticationService);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info)
        {
            var authorizationResult = await authenticator.SignInAsync(info);
            updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            // TODO: use connection info to check correctness
            return Task.FromResult(authenticator.IsLogged);
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
            => Task.FromResult(updatedInfo);

        public ConnectionTypeExternalDto GetConnectionType()
        {
            var type = new ConnectionTypeExternalDto
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
                ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                {
                    new ObjectiveTypeExternalDto
                    {
                        ExternalId = Constants.ISSUE_TYPE,
                        Name = "Issue",
                    },
                },
            };

            return type;
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => await Bim360ConnectionContext.CreateContext(info);
    }
}
