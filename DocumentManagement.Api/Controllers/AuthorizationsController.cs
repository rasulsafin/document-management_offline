using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Threading.Tasks;
using static DocumentManagement.Api.Validators.ServiceResponsesValidator;

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
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await service.GetAllRoles();
            return ValidateCollection(roles);
        }

        [HttpPost]
        [Route("user/roles")]
        public async Task<IActionResult> AddRole([FromQuery] int userID, [FromQuery] string role)
        {
            try
            {
                var added = await service.AddRole(new ID<UserDto>(userID), role);
                return Ok(added);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidDataException)
            {
                return BadRequest();
            }
        }

        [HttpDelete]
        [Route("user/roles")]
        public async Task<IActionResult> RemoveRole([FromQuery] int userID, [FromQuery] string role)
        {
            var removed = await service.RemoveRole(new ID<UserDto>(userID), role);
            return ValidateFoundRelatedResult(removed);
        }

        [HttpGet]
        [Route("user/userID/roles")]
        public async Task<IActionResult> GetUserRoles([FromRoute] int userID)
        {
            var roles = await service.GetUserRoles(new ID<UserDto>(userID));
            return ValidateCollection(roles);
        }

        [HttpGet]
        [Route("user/roles")]
        public async Task<IActionResult> IsInRole([FromQuery] int userID, [FromQuery] string role)
        {
            var isInRole = await service.IsInRole(new ID<UserDto>(userID), role);
            return Ok(isInRole);
        }
    }
}
