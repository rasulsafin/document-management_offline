using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProConnection : IConnection
    {
        private readonly ILogger<MrsProConnection> logger;
        private readonly AuthenticationService authService;
        private readonly UsersService userService;
        private readonly Func<MrsProConnectionContext> getContext;

        public MrsProConnection(
            ILogger<MrsProConnection> logger,
            AuthenticationService authService,
            UsersService userService,
            Func<MrsProConnectionContext> getContext)
        {
            this.logger = logger;
            this.authService = authService;
            this.userService = userService;
            this.getContext = getContext;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                var authStatus = await authService.SignInAsync(
                    info.AuthFieldValues[AUTH_EMAIL],
                    info.AuthFieldValues[AUTH_PASS],
                    info.AuthFieldValues[COMPANY_CODE]);

                return authStatus;
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = ex.Message };
            }
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => getContext();

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            // TODO: Get Status
            throw new NotImplementedException();
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            return new MrsProStorage();
        }

        public async Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto
                {
                   ExternalId = ISSUE_TYPE,
                   Name = "Замечание",
                },
                new ObjectiveTypeExternalDto
                {
                   ExternalId = ELEMENT_TYPE,
                   Name = "Элемент проекта",
                },
            };

            var currentUser = await userService.GetMe();
            info.UserExternalID = currentUser.Id;

            // TODO: Dynamic fields
            return info;
        }
    }
}
