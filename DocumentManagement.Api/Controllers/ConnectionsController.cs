using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Connection;
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
        private IConnection connection;

        public ConnectionsController(IConnectionService connectionService, IConnection connection)
        {
            service = connectionService;
            this.connection = connection;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConnectionInfoToCreateDto connectionInfo)
        {
            var connectionInfoId = await service.Add(connectionInfo);
            return ValidateId(connectionInfoId);
        }

        [HttpGet]
        [Route("connect/{userID}")]
        public async Task<IActionResult> Connect([FromRoute] int userID)
        {
            var result = await service.Connect(new ID<UserDto>(userID));
            return Ok(result);
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<IActionResult> Get([FromRoute] int userID)
        {
            var connectionInfoDto = await service.Get(new ID<UserDto>(userID));
            return ValidateFoundObject(connectionInfoDto);
        }

        [HttpPost]
        [Route("status/{userID}")]
        public async Task<IActionResult> GetRemoteConnectionStatus([FromRoute] int userID)
        {
            var status = await service.GetRemoteConnectionStatus(new ID<UserDto>(userID));
            return Ok(status);
        }

        // TODO: Syncronization!

        [HttpPost]
        [Route("syncronization")]
        public async Task<IActionResult> StartSyncronization()
        {
            var needConnect = await connection.IsAuthDataCorrect();
            if (!needConnect)
            {
                await connection.Connect(new ConnectionInfoDto()
                {
                    ConnectionType = new ConnectionTypeDto() { },
                    ID = new ID<ConnectionInfoDto>(2),
                });
            }

            await connection.StartSyncronization();
            return Ok();
        }
    }
}
