using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private IConnectionService service;

        public ConnectionsController(IConnectionService connectionService) => service = connectionService;

        [HttpGet]
        public async Task<IEnumerable<RemoteConnectionInfo>> GetAvailableConnections() => await service.GetAvailableConnections();

        [HttpPost]
        public async Task LinkRemoteConnection(RemoteConnectionToCreate connectionInfo) => await service.LinkRemoteConnection(connectionInfo);

        [HttpGet]
        public async Task<ConnectionStatus> GetRemoteConnectionStatus() => await service.GetRemoteConnectionStatus();

        [HttpPut]
        public async Task Reconnect(RemoteConnectionToCreate connectionInfo) => await service.Reconnect(connectionInfo);

        [HttpGet]
        public async Task<RemoteConnectionInfo> GetCurrentConnection() => await service.GetCurrentConnection();

        [HttpGet]
        public async Task<IEnumerable<EnumVariant>> GetEnumVariants(string dynamicFieldKey) => await service.GetEnumVariants(dynamicFieldKey);
    }
}
