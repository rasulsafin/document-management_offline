using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LementProConnection> logger;
        private readonly HttpConnection connector;
        private readonly AuthenticationService authenticationService;
        private readonly HttpRequestUtility requestUtility;

        private ConnectionInfoExternalDto updatedInfo;

        public LementProConnection(ILogger<LementProConnection> logger)
        {
            this.logger = logger;
            connector = new HttpConnection();
            requestUtility = new HttpRequestUtility(connector);
            authenticationService = new AuthenticationService(requestUtility);
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

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("Connect started with info: {@ConnectionInfo}", info);
            var authorizationResult = await authenticationService.SignInAsync(info);
            logger.LogDebug("SignInAsync returned: {@AuthResult}", authorizationResult);
            logger.LogInformation("User {UserID} connection result: {@AuthStatus}", info.UserExternalID, authorizationResult.authStatus);
            updatedInfo = authorizationResult.updatedInfo;
            return authorizationResult.authStatus;
        }

        public async Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("UpdateConnectionInfo started with info: {@ConnectionInfo}", info);
            if (updatedInfo == null)
                await Connect(info);

            updatedInfo.ConnectionType.ObjectiveTypes = await GetTypesAsync();
            updatedInfo.UserExternalID = updatedInfo.AuthFieldValues[AUTH_NAME_LOGIN];
            logger.LogDebug("Updated info: {@ConnectionInfo}", info);

            return updatedInfo;
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("GetStatus started with info: {@ConnectionInfo}", info);
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

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("GetContext started with info: {@ConnectionInfo}", info);
            return await LementProConnectionContext.CreateContext(info);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("GetStorage started with info: {@ConnectionInfo}", info);
            await Connect(info);
            return new LementProConnectionStorage(requestUtility);
        }

        private async Task<ICollection<ObjectiveTypeExternalDto>> GetTypesAsync()
        {
            logger.LogTrace("GetTypesAsync started with info");
            using var taskService = new TasksService(requestUtility, new CommonRequestsUtility(requestUtility));
            var typesModels = await taskService.GetTasksTypesAsync();
            logger.LogDebug("Found types: {@Types}", typesModels);
            var types = new List<ObjectiveTypeExternalDto>();

            // TODO: check why only 1 type.
            var defaultType = typesModels.FirstOrDefault();
            if (defaultType != default)
                types.Add(defaultType.ToObjectiveTypeExternal());

            logger.LogDebug("Mapped types: {@Types}", types);
            return types;
        }
    }
}
