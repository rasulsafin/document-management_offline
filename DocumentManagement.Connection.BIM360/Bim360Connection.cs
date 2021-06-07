using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Utils.Extensions;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public class Bim360Connection : IConnection
    {
        private readonly AuthenticationService authenticationService;
        private readonly Bim360Storage storage;
        private readonly IFactory<ConnectionInfoExternalDto, IConnectionContext> contextFactory;
        private readonly TokenHelper tokenHelper;
        private readonly Authenticator authenticator;

        public Bim360Connection(
            Authenticator authenticator,
            AuthenticationService authenticationService,
            Bim360Storage storage,
            IFactory<ConnectionInfoExternalDto, IConnectionContext> contextFactory,
            TokenHelper tokenHelper)
        {
            this.authenticator = authenticator;
            this.authenticationService = authenticationService;
            this.storage = storage;
            this.contextFactory = contextFactory;
            this.tokenHelper = tokenHelper;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            var authorizationResult = await authenticator.SignInAsync(info, token);
            return authorizationResult.authStatus;
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            try
            {
                var token = info.GetAuthValue(Constants.TOKEN_AUTH_NAME);

                if (string.IsNullOrWhiteSpace(token))
                {
                    return Task.FromResult(
                        new ConnectionStatusDto
                        {
                            Status = RemoteConnectionStatus.Error,
                            Message = "Token is empty",
                        });
                }

                var jwt = new JwtSecurityToken(token);
                return Task.FromResult(
                    DateTime.UtcNow.AddMinutes(1) < jwt.ValidTo
                        ? new ConnectionStatusDto
                        {
                            Status = RemoteConnectionStatus.OK,
                            Message = "Token is still valid",
                        }
                        : new ConnectionStatusDto
                        {
                            Status = RemoteConnectionStatus.NeedReconnect,
                            Message = "Token expired",
                        });
            }
            catch
            {
                return Task.FromResult(
                    new ConnectionStatusDto
                    {
                        Status = RemoteConnectionStatus.Error,
                        Message = "Token is invalid",
                    });
            }
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
            tokenHelper.SetToken(info.AuthFieldValues[Constants.TOKEN_AUTH_NAME]);
            info.UserExternalID = (await authenticationService.GetMe()).UserId;
            tokenHelper.SetUserID(info.UserExternalID);
            return info;
        }

        public Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => Task.FromResult(contextFactory.Create(info));

        public Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            tokenHelper.SetInfo(info.UserExternalID, info.AuthFieldValues[Constants.TOKEN_AUTH_NAME]);
            return Task.FromResult<IConnectionStorage>(storage);
        }

        private ICollection<EnumerationTypeExternalDto> GetEnumerationTypes()
            => new List<EnumerationTypeExternalDto> { Constants.STANDARD_NG_TYPES.Value };
    }
}
