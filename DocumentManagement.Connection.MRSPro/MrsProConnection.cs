using System;
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

        public MrsProConnection(ILogger<MrsProConnection> logger, AuthenticationService authService)
        {
            this.logger = logger;
            this.authService = authService;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            logger.LogTrace("SignInAsync started with info: {@ConnectionInfo}", info);
            MrsProHttpConnection.CompanyCode = info.AuthFieldValues[COMPANY_CODE];
            return await authService.Connect(info.AuthFieldValues[AUTH_EMAIL], info.AuthFieldValues[AUTH_PASS]);
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            return new MrsProConnectionContext();
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            return new MrsProStorage();
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }
    }
}
