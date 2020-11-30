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
        [Route("role/user")]
        public async Task AddRole(int userID, string role) => await service.AddRole(new ID<UserDto>(userID), role);

        [HttpDelete]
        [Route("roles")]
        public async Task RemoveRole(ID<UserDto> userID, string role) => await service.RemoveRole(userID, role);

        [HttpGet]
        [Route("roles/user")]
        public async Task<IEnumerable<string>> GetUserRoles(int userID) => await service.GetUserRoles(new ID<UserDto>(userID));

        [HttpGet]
        [Route("isinrole/user")]
        public async Task<bool> IsInRole(ID<UserDto> userID, string role) => await service.IsInRole(userID, role);
    }
}
