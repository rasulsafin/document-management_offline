using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private IConnectionService service;
        private ISyncService syncService;

        public ConnectionsController(IConnectionService connectionService, ISyncService syncService)
        {
            service = connectionService;
            this.syncService = syncService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableConnections()
        {
            var availableConnections = await service.GetAvailableConnections();
            return ValidateCollection(availableConnections);
        }

        [HttpPost]
        public async Task<IActionResult> LinkRemoteConnection(RemoteConnectionToCreateDto connectionInfo)
        {
            var linked = await service.LinkRemoteConnection(connectionInfo);
            return Forbid();
        }

        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> GetRemoteConnectionStatus()
        {
            var status = await service.GetRemoteConnectionStatus();
            return Forbid();
        }

        [HttpPut]
        public async Task<IActionResult> Reconnect(RemoteConnectionToCreateDto connectionInfo)
        {
            var reconnected = await service.Reconnect(connectionInfo);
            return Forbid();
        }

        [HttpGet]
        [Route("current")]
        public async Task<IActionResult> GetCurrentConnection(int userId)
        {
            var connection = await service.GetCurrentConnection(new ID<UserDto>(userId));
            return ValidateFoundObject(connection);
        }

        [HttpHead]
        [Route("syncStart")]
        public IActionResult StartSynchronize()
        {
            syncService.StartSync();
            return Accepted();
        }

        [HttpGet]
        [Route("progress")]
        public IActionResult GetProgressSync()
        {
            var progress = syncService.GetProgressSync();
            return ValidateFoundObject(progress);
        }

        [HttpHead]
        [Route("syncStop")]
        public IActionResult StopSynchronize()
        {
            syncService.StopSync();
            return Accepted();
        }

        //[HttpGet]
        //[Route("variants")]
        //public async Task<IActionResult> GetEnumVariants(string dynamicFieldKey)
        //{
        //    var variants = await service.GetEnumVariants(dynamicFieldKey);
        //    return Forbid();
        //}
    }
}
