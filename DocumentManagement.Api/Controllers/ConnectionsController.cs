using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private readonly IConnectionService service;

        public ConnectionsController(IConnectionService connectionService) => service = connectionService;

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

        [HttpGet]
        [Route("status/{userID}")]
        public async Task<IActionResult> GetRemoteConnectionStatus([FromRoute] int userID)
        {
            var status = await service.GetRemoteConnectionStatus(new ID<UserDto>(userID));
            return ValidateFoundObject(status);
        }

        [HttpGet]
        [Route("enumerationValues")]
        public async Task<IActionResult> GetEnumerationVariants([FromQuery] int userID, [FromQuery]int enumerationTypeID)
        {
            var result = await service.GetEnumerationVariants(new ID<UserDto>(userID), new ID<EnumerationTypeDto>(enumerationTypeID));
            return ValidateFoundObject(result);
        }

        [HttpGet]
        [Route("synchronization/start/{userID}")]
        public async Task<IActionResult> Synchronize([FromRoute] int userID)
        {
            var result = await service.Synchronize(new ID<UserDto>(userID));
            return Ok(result);
        }

        [HttpGet]
        [Route("synchronization/{id}/status")]
        public async Task<IActionResult> IsSynchronizationComplete([FromRoute] string id)
        {
            var result = await service.IsSynchronizationComplete(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("synchronization/{id}/result")]
        public async Task<IActionResult> GetSynchronizationResult([FromRoute] string id)
        {
            var result = await service.GetSynchronizationResult(id);
            return Ok(result);
        }
    }
}
