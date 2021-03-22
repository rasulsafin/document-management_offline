using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public class Bim360Connection : IConnection, IDisposable
    {
        private readonly AuthenticationService authenticationService;
        private readonly Authenticator authenticator;
        private readonly ForgeConnection connection;

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
            return authorizationResult.authStatus;
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            return Task.FromResult(
                new ConnectionStatusDto { Status = RemoteConnectionStatus.OK });
        }

        public async Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            info.EnumerationTypes = GetEnumerationTypes();
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto
                {
                   ExternalId = Constants.ISSUE_TYPE,
                   Name = "Issue",
                },
            };
            info.ConnectionType.ObjectiveTypes.First().DefaultDynamicFields = new List<DynamicFieldExternalDto>
            {
                new DynamicFieldExternalDto
                {
                    ExternalID =
                        typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.NgIssueTypeID)),
                    Name = "Type",
                    Type = DynamicFieldType.ENUM,
                    Value = Constants.UNDEFINED_NG_TYPE.ExternalID,
                },
            };
            connection.Token = info.AuthFieldValues[Constants.TOKEN_AUTH_NAME];
            info.UserExternalID = (await authenticationService.GetMe()).UserId;
            return info;
        }

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
            };

            return type;
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => await Bim360ConnectionContext.CreateContext(info);

        public Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
            => Task.FromResult<IConnectionStorage>(Bim360Storage.Create(info));

        private ICollection<EnumerationTypeExternalDto> GetEnumerationTypes()
            => new List<EnumerationTypeExternalDto> { Constants.STANDARD_NG_TYPES.Value };
    }
}
