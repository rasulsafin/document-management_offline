using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.Bim360.Forge;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.External.Extensions;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360
{
    internal class Bim360Connection : IConnection
    {
        private readonly AuthenticationService authenticationService;
        private readonly Bim360Storage storage;
        private readonly IFactory<ConnectionInfoExternalDto, IConnectionContext> contextFactory;
        private readonly TokenHelper tokenHelper;
        private readonly EnumerationTypeCreator typeCreator;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly LocationEnumCreator locationEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;
        private readonly Authenticator authenticator;

        public Bim360Connection(
            Authenticator authenticator,
            AuthenticationService authenticationService,
            Bim360Storage storage,
            IFactory<ConnectionInfoExternalDto, IConnectionContext> contextFactory,
            TokenHelper tokenHelper,
            EnumerationTypeCreator typeCreator,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            LocationEnumCreator locationEnumCreator,
            AssignToEnumCreator assignToEnumCreator)
        {
            this.authenticator = authenticator;
            this.authenticationService = authenticationService;
            this.storage = storage;
            this.contextFactory = contextFactory;
            this.tokenHelper = tokenHelper;
            this.typeCreator = typeCreator;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
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

                if (DateTime.UtcNow.Add(ForgeConnection.MIN_TOKEN_LIFE) > jwt.ValidTo)
                {
                    return new ConnectionStatusDto
                    {
                        Status = RemoteConnectionStatus.NeedReconnect,
                        Message = "Token expired",
                    };
                }

                tokenHelper.SetInfo(info.UserExternalID, token);

                if (await authenticationService.GetMeAsync() == null)
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
            info.UserExternalID = (await authenticationService.GetMeAsync()).UserId;
            tokenHelper.SetClientID(info.UserExternalID);
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
            var typesSubtypes = await typeCreator.Create(subtypeEnumCreator);
            if (typesSubtypes.EnumerationValues.Count == 0)
                throw new TypeAccessException("You have no access to issue types.");

            var rootCauses = await typeCreator.Create(rootCauseEnumCreator);
            var location = await typeCreator.Create(locationEnumCreator);
            var assignTo = await typeCreator.Create(assignToEnumCreator);

            info.EnumerationTypes = new List<EnumerationTypeExternalDto>
            {
                typesSubtypes,
                rootCauses,
                location,
                assignTo,
            };

            var issueType = new ObjectiveTypeExternalDto
            {
                ExternalId = Constants.ISSUE_TYPE,
                Name = MrsConstants.ISSUE_TYPE_NAME,
                DefaultDynamicFields = new List<DynamicFieldExternalDto>
                {
                    DynamicFieldUtilities.CreateField(
                        typesSubtypes.EnumerationValues.First().ExternalID,
                        subtypeEnumCreator),
                    DynamicFieldUtilities.CreateField(
                        rootCauseEnumCreator.NullID,
                        rootCauseEnumCreator),
                    DynamicFieldUtilities.CreateField(
                        locationEnumCreator.NullID,
                        locationEnumCreator),
                    DynamicFieldUtilities.CreateField(
                        assignToEnumCreator.NullID,
                        assignToEnumCreator),
                    new ()
                    {
                        ExternalID = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.LocationDescription),
                        Type = DynamicFieldType.STRING,
                        Name = MrsConstants.LOCATION_DETAILS_FIELD_NAME,
                        Value = string.Empty,
                    },
                    new ()
                    {
                        ExternalID = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.Answer),
                        Type = DynamicFieldType.STRING,
                        Name = MrsConstants.RESPONSE_FIELD_NAME,
                        Value = string.Empty,
                    },
                },
            };

            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto> { issueType };
        }
    }
}
