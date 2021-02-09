using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService service;

        public UsersController(IUserService userService) => service = userService;

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await service.GetAllUsers();
            return ValidateCollection(users);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UserToCreateDto data)
        {
            try
            {
                var userId = await service.Add(data);
                return ValidateId(userId);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete]
        [Route("{userID}")]
        public async Task<IActionResult> Delete([FromRoute] int userID)
        {
            var deleted = await service.Delete(new ID<UserDto>(userID));
            return ValidateFoundRelatedResult(deleted);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserDto user)
        {
            var updated = await service.Update(user);
            if (!updated)
                return BadRequest();

            return Ok();
        }

        [HttpPost]
        [Route("{userID}/password")]
        public async Task<IActionResult> VerifyPassword([FromRoute] int userID, [FromBody] string password)
        {
            try
            {
                var verifyResult = await service.VerifyPassword(new ID<UserDto>(userID), password);
                return Ok(verifyResult);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("{userID}/password")]
        public async Task<IActionResult> UpdatePassword([FromRoute] int userID, [FromBody] string newPass)
        {
            var updated = await service.UpdatePassword(new ID<UserDto>(userID), newPass);
            if (!updated)
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<IActionResult> Find([FromRoute] int userID)
        {
            var foundUser = await service.Find(new ID<UserDto>(userID));
            return ValidateFoundObject(foundUser);
        }

        [HttpGet]
        [Route("find")]
        public async Task<IActionResult> Find([FromQuery] string login)
        {
            var foundUser = await service.Find(login);
            return ValidateFoundObject(foundUser);
        }

        [HttpGet]
        [Route("{userID}/exists")]
        public async Task<IActionResult> Exists([FromRoute] int userID)
        {
            var exists = await service.Exists(new ID<UserDto>(userID));
            return Ok(exists);
        }

        [HttpGet]
        [Route("exists")]
        public async Task<IActionResult> Exists([FromQuery] string login)
        {
            var exists = await service.Exists(login);
            return Ok(exists);
        }
    }
}
