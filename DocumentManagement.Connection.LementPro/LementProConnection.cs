using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Connection.Utils.Extensions;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro
{
    public class LementProConnection : IConnection, IDisposable
    {
        private readonly ILogger<LementProConnection> logger;
        private readonly HttpConnection connector;
        private readonly AuthenticationService authenticationService;
        private readonly HttpRequestUtility requestUtility;
        private readonly Factory<LementProConnectionContext> contextFactory;
        private readonly Factory<LementProConnectionStorage> storageFactory;
        private readonly TasksService tasksService;

        private ConnectionInfoExternalDto updatedInfo;

        public LementProConnection(
            ILogger<LementProConnection> logger,
            HttpConnection connector,
            AuthenticationService authenticationService,
            HttpRequestUtility requestUtility,
            Factory<LementProConnectionContext> contextFactory,
            Factory<LementProConnectionStorage> storageFactory,
            TasksService tasksService)
        {
            this.logger = logger;
            this.connector = connector;
            this.authenticationService = authenticationService;
            this.requestUtility = requestUtility;
            this.contextFactory = contextFactory;
            this.storageFactory = storageFactory;
            this.tasksService = tasksService;
            logger.LogTrace("LementProConnection created");
        }

        public void Dispose()
        {
            connector.Dispose();
            authenticationService.Dispose();
            requestUtility.Dispose();
            GC.SuppressFinalize(this);
            logger.LogTrace("LementProConnection disposed");
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            logger.LogTrace("Connect started with info: {@ConnectionInfo}", info);
            SetToken(info);
            var authorizationResult = await authenticationService.SignInAsync(info);
            logger.LogDebug("SignInAsync returned: {@AuthResult}", authorizationResult);
            logger.LogInformation("User {UserID} connection result: {@AuthStatus}", info.UserExternalID, authorizationResult.authStatus);
            updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
        }

        public async Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("UpdateConnectionInfo started with info: {@ConnectionInfo}", info);
            SetToken(info);
            if (updatedInfo == null)
                await Connect(info, default);

            updatedInfo.ConnectionType.ObjectiveTypes = await GetTypesAsync();
            updatedInfo.UserExternalID = updatedInfo.AuthFieldValues[AUTH_NAME_LOGIN];
            logger.LogDebug("Updated info: {@ConnectionInfo}", info);

            return updatedInfo;
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("GetStatus started with info: {@ConnectionInfo}", info);
            SetToken(info);
            var isCorrect = authenticationService.IsAuthorisationAccessValid(info);
            logger.LogDebug("Auth data correct: {@Result}", isCorrect);

            var status = new ConnectionStatusDto
            {
                Status = isCorrect
                ? RemoteConnectionStatus.OK
                : RemoteConnectionStatus.NeedReconnect,
            };

            return Task.FromResult(status);
        }

        public Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("GetContext started with info: {@ConnectionInfo}", info);
            SetToken(info);
            return Task.FromResult<IConnectionContext>(contextFactory.Create());
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("GetStorage started with info: {@ConnectionInfo}", info);
            SetToken(info);
            await Connect(info, default);
            return storageFactory.Create();
        }

        private async Task<ICollection<ObjectiveTypeExternalDto>> GetTypesAsync()
        {
            logger.LogTrace("GetTypesAsync started with info");
            var typesModels = await tasksService.GetTasksTypesAsync();
            logger.LogDebug("Found types: {@Types}", typesModels);
            var types = new List<ObjectiveTypeExternalDto>();

            // TODO: check why only 1 type.
            var defaultType = typesModels.FirstOrDefault();
            if (defaultType != default)
                types.Add(defaultType.ToObjectiveTypeExternal());

            logger.LogDebug("Mapped types: {@Types}", types);
            return types;
        }

        private void SetToken(ConnectionInfoExternalDto info)
            => requestUtility.Token = info.GetAuthValue(AUTH_NAME_TOKEN);
    }
}
