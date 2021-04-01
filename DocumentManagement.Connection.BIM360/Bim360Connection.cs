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
        private readonly Bim360Storage storage;
        private readonly Authenticator authenticator;
        private readonly ForgeConnection connection;

        public Bim360Connection(
            ForgeConnection connection,
            Authenticator authenticator,
            AuthenticationService authenticationService,
            Bim360Storage storage)
        {
            this.connection = connection;
            this.authenticator = authenticator;
            this.authenticationService = authenticationService;
            this.storage = storage;
        }

        public void Dispose()
        {
            connection.Dispose();
            GC.SuppressFinalize(this);
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

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => await Bim360ConnectionContext.CreateContext(info);

        public Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            connection.Token = info.AuthFieldValues[Constants.TOKEN_AUTH_NAME];
            return Task.FromResult<IConnectionStorage>(storage);
        }

        private ICollection<EnumerationTypeExternalDto> GetEnumerationTypes()
            => new List<EnumerationTypeExternalDto> { Constants.STANDARD_NG_TYPES.Value };
    }
}
