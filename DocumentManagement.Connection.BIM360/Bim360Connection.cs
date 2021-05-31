using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public class Bim360Connection : IConnection
    {
        private readonly AuthenticationService authenticationService;
        private readonly Bim360Storage storage;
        private readonly IFactory<IServiceScope, Bim360ConnectionContext> contextFactory;
        private readonly Authenticator authenticator;
        private readonly ForgeConnection connection;
        private readonly Lazy<IServiceScope> contextScopeContainer;

        public Bim360Connection(
            ForgeConnection connection,
            Authenticator authenticator,
            AuthenticationService authenticationService,
            Bim360Storage storage,
            IServiceScopeFactory scopeFactory,
            IFactory<IServiceScope, Bim360ConnectionContext> contextFactory)
        {
            this.connection = connection;
            this.authenticator = authenticator;
            this.authenticationService = authenticationService;
            this.storage = storage;
            this.contextFactory = contextFactory;

            contextScopeContainer = new Lazy<IServiceScope>(scopeFactory.CreateScope);
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            var authorizationResult = await authenticator.SignInAsync(info, token);
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

        public Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            connection.Token = info.AuthFieldValues[Constants.TOKEN_AUTH_NAME];
            return Task.FromResult<IConnectionContext>(contextFactory.Create(contextScopeContainer.Value));
        }

        public Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            connection.Token = info.AuthFieldValues[Constants.TOKEN_AUTH_NAME];
            return Task.FromResult<IConnectionStorage>(storage);
        }

        private ICollection<EnumerationTypeExternalDto> GetEnumerationTypes()
            => new List<EnumerationTypeExternalDto> { Constants.STANDARD_NG_TYPES.Value };
    }
}
