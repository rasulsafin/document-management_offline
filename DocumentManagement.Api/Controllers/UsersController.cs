using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService service;

        public UsersController(IUserService userService) => service = userService;

        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetAllUsers() => await service.GetAllUsers();

        [HttpPost]
        public async Task<ID<UserDto>> Add([FromBody] UserToCreateDto data) => await service.Add(data);

        [HttpDelete]
        [Route("{userID}")]
        public async Task Delete([FromRoute] int userID) => await service.Delete(new ID<UserDto>(userID));

        [HttpPut]
        public async Task Update([FromBody] UserDto user) => await service.Update(user);

        [HttpGet]
        [Route("{userID}/password")]
        public async Task<bool> VerifyPassword([FromRoute] int userID, [FromBody] string password) => await service.VerifyPassword(new ID<UserDto>(userID), password);

        [HttpPut]
        [Route("{userID}/password")]
        public async Task UpdatePassword([FromRoute] int userID, [FromBody] string newPass) => await service.UpdatePassword(new ID<UserDto>(userID), newPass);

        [HttpGet]
        [Route("{userID}")]
        public async Task<UserDto> Find([FromRoute] int userID) => await service.Find(new ID<UserDto>(userID));

        [HttpGet]
        [Route("login")]
        public async Task<UserDto> Find([FromQuery] string login) => await service.Find(login);

        [HttpGet]
        [Route("{userID}/exists")]
        public async Task<bool> Exists([FromRoute] int userID) => await service.Exists(new ID<UserDto>(userID));

        [HttpGet]
        [Route("login/exists")]
        public async Task<bool> Exists([FromQuery] string login) => await service.Exists(login);

        //public async Task<User> GetCurrentUser() => await userService.GetCurrentUser();
    }
}
