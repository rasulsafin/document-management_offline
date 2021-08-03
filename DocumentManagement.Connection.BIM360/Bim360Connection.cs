using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Utils.Extensions;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    internal class Bim360Connection : IConnection
    {
        private readonly AuthenticationService authenticationService;
        private readonly Bim360Storage storage;
        private readonly IFactory<ConnectionInfoExternalDto, IConnectionContext> contextFactory;
        private readonly TokenHelper tokenHelper;
        private readonly TypeDFHelper typeDfHelper;
        private readonly Authenticator authenticator;

        public Bim360Connection(
            Authenticator authenticator,
            AuthenticationService authenticationService,
            Bim360Storage storage,
            IFactory<ConnectionInfoExternalDto, IConnectionContext> contextFactory,
            TokenHelper tokenHelper,
            TypeDFHelper typeDfHelper)
        {
            this.authenticator = authenticator;
            this.authenticationService = authenticationService;
            this.storage = storage;
            this.contextFactory = contextFactory;
            this.tokenHelper = tokenHelper;
            this.typeDfHelper = typeDfHelper;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            var authorizationResult = await authenticator.SignInAsync(info, token);
            return authorizationResult.authStatus;
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            if (!await Bim360WebFeatures.CanPingAutodesk())
            {
                return new ConnectionStatusDto
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = "Failed to ping the server",
                };
            }

            try
            {
                var token = info.GetAuthValue(Constants.TOKEN_AUTH_NAME);

                if (string.IsNullOrWhiteSpace(token))
                {
                    return
                        new ConnectionStatusDto
                        {
                            Status = RemoteConnectionStatus.Error,
                            Message = "Token is empty",
                        };
                }

                var jwt = new JwtSecurityToken(token);

                if (DateTime.UtcNow.AddMinutes(1) > jwt.ValidTo)
                {
                    return new ConnectionStatusDto
                    {
                        Status = RemoteConnectionStatus.NeedReconnect,
                        Message = "Token expired",
                    };
                }

                tokenHelper.SetInfo(info.UserExternalID, token);

                if (await authenticationService.GetMe() == null)
                {
                    return new ConnectionStatusDto
                    {
                        Status = RemoteConnectionStatus.Error,
                        Message = "Token is invalid",
                    };
                }

                return new ConnectionStatusDto
                {
                    Status = RemoteConnectionStatus.OK,
                    Message = "Token is still valid",
                };
            }
            catch
            {
                return
                    new ConnectionStatusDto
                    {
                        Status = RemoteConnectionStatus.Error,
                        Message = "Token is invalid",
                    };
            }
        }

        public async Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            await SetIssueType(info);
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

        private async Task SetIssueType(ConnectionInfoExternalDto info)
        {
            var issueType = new ObjectiveTypeExternalDto
            {
                ExternalId = Constants.ISSUE_TYPE,
                Name = "Issue",
            };

            var enumTypes = new List<EnumerationTypeExternalDto>();
            var typesSubtypes = await typeDfHelper.GetTypeEnumeration();
            if (typesSubtypes.EnumerationValues.Count == 0)
                throw new TypeAccessException("You have no access to issue types.");

            enumTypes.Add(typesSubtypes);
            info.EnumerationTypes = enumTypes;

            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto> { issueType };
            issueType.DefaultDynamicFields = new List<DynamicFieldExternalDto>
            {
                TypeDFHelper.CreateField(typesSubtypes.EnumerationValues.First().ExternalID),
            };
        }
    }
}
