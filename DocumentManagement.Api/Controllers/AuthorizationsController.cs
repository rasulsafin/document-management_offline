using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationsController : ControllerBase
    {
        private IAuthorizationService service;

        public AuthorizationsController(IAuthorizationService authorizationService) => service = authorizationService;

        [HttpGet]
        public async Task<IEnumerable<string>> GetAllRoles() => await service.GetAllRoles();

        [HttpPost]
        public async Task AddRole(ID<User> userID, string role) => await service.AddRole(userID, role);

        [HttpDelete]
        public async Task RemoveRole(ID<User> userID, string role) => await service.RemoveRole(userID, role);

        [HttpGet]
        public async Task<IEnumerable<string>> GetUserRoles(ID<User> userID) => await service.GetUserRoles(userID);

        [HttpGet]
        public async Task<bool> IsInRole(ID<User> userID, string role) => await service.IsInRole(userID, role);
    }
}
