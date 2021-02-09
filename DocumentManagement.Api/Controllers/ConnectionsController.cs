using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private IConnectionService service;

        public ConnectionsController(IConnectionService connectionService) => service = connectionService;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConnectionInfoToCreateDto connectionInfo)
        {
            var result = await service.Add(connectionInfo);
            return Ok(result);
        }

        [HttpGet]
        [Route("connect/{userID}")]
        public async Task<IActionResult> Connect([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<IActionResult> Get([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> GetRemoteConnectionStatus()
        {
            throw new System.NotImplementedException();
        }

        [HttpPost]
        [Route("startsyncronization/{userID}")]
        public async Task<IActionResult> StartSyncronization([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost]
        [Route("stopsyncronization/{userID}")]
        public async Task<IActionResult> StopSyncronization([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }
    }
}
