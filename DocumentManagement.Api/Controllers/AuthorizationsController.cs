using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationsController : ControllerBase
    {
        private IAuthorizationService service;

        public AuthorizationsController(IAuthorizationService authorizationService) => service = authorizationService;

        [HttpGet]
        [Route("roles")]
        public async Task<IEnumerable<string>> GetAllRoles() => await service.GetAllRoles();

        [HttpPost]
        [Route("user/roles")]
        public async Task AddRole([FromQuery] int userID, [FromQuery] string role) => await service.AddRole(new ID<UserDto>(userID), role);

        [HttpDelete]
        [Route("user/roles")]
        public async Task RemoveRole([FromQuery] int userID, [FromQuery] string role) => await service.RemoveRole(new ID<UserDto>(userID), role);

        [HttpGet]
        [Route("user/userID/roles")]
        public async Task<IEnumerable<string>> GetUserRoles([FromRoute] int userID) => await service.GetUserRoles(new ID<UserDto>(userID));

        [HttpGet]
        [Route("user/roles")]
        public async Task<bool> IsInRole([FromQuery] int userID, [FromQuery] string role) => await service.IsInRole(new ID<UserDto>(userID), role);
    }
}
