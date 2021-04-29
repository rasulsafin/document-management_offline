using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class AuthenticationService
    {
        private readonly ILogger<AuthenticationService> logger;
        private readonly HttpRequestUtility requestUtility;

        public AuthenticationService(HttpRequestUtility requestUtility, ILogger<AuthenticationService> logger)
        {
            this.requestUtility = requestUtility;
            this.logger = logger;
            logger.LogTrace("AuthenticationService created");
        }

        public async Task<(ConnectionStatusDto authStatus, ConnectionInfoExternalDto updatedInfo)> SignInAsync(ConnectionInfoExternalDto info)
        {
            logger.LogTrace("SignInAsync started with info: {@ConnectionInfo}", info);

            var login = info.AuthFieldValues[AUTH_NAME_LOGIN];
            var password = info.AuthFieldValues[AUTH_NAME_PASSWORD];

            var (token, expires) = await requestUtility.Connect(login, password);

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expires))
            {
                logger.LogError("Connection data is empty");
                var errorStatus = new ConnectionStatusDto
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = "Connection with given credentials failed",
                };
                return (errorStatus, null);
            }

            info.SetAuthValue(AUTH_NAME_TOKEN, token);
            info.SetAuthValue(AUTH_NAME_END, token);
            requestUtility.Token = token;
            requestUtility.EnsureAccessValidAsync = () => EnsureAccessValidAsync(info);
            var successStatus = new ConnectionStatusDto { Status = RemoteConnectionStatus.OK };

            logger.LogDebug("Result info: {@ConnectionInfo}", info);
            return (successStatus, info);
        }

        public async Task EnsureAccessValidAsync(ConnectionInfoExternalDto connectionInfo)
        {
            logger.LogTrace("EnsureAccessValidAsync started with info: {@ConnectionInfo}", connectionInfo);
            if (IsAuthorisationAccessValid(connectionInfo))
                return;

            await SignInAsync(connectionInfo);
        }

        internal bool IsAuthorisationAccessValid(ConnectionInfoExternalDto connectionInfo)
        {
            logger.LogTrace("IsAuthorisationAccessValid started with info: {@ConnectionInfo}", connectionInfo);
            if (string.IsNullOrWhiteSpace(connectionInfo.GetAuthValue(AUTH_NAME_TOKEN)))
                return false;

            if (!DateTime.TryParse(connectionInfo.GetAuthValue(AUTH_NAME_END), out var parsedExpiresDate))
                return false;

            if (parsedExpiresDate < DateTime.Now)
                return false;

            return true;
        }
    }
}
