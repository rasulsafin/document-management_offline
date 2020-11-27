using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationsController : ControllerBase
    {
        private IAuthorizationService service;

        public AuthorizationsController(IAuthorizationService authorizationService) => service = authorizationService;

        [HttpGet]
        [Route("Get/Roles")]
        public async Task<IEnumerable<string>> GetAllRoles() => await service.GetAllRoles();

        [HttpPost]
        [Route("GetUserRoles")]
        public async Task AddRole(ID<User> userID, string role) => await service.AddRole(userID, role);

        [HttpDelete]
        [Route("Remove/Roles")]
        public async Task RemoveRole(ID<User> userID, string role) => await service.RemoveRole(userID, role);

        [HttpGet]
        [Route("Get/Roles/User")]
        public async Task<IEnumerable<string>> GetUserRoles(ID<User> userID) => await service.GetUserRoles(userID);

        [HttpGet]
        [Route("IsInRole/User")]
        public async Task<bool> IsInRole(ID<User> userID, string role) => await service.IsInRole(userID, role);
    }
}
