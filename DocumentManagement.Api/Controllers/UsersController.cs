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
        public async Task<ID<UserDto>> Add(UserToCreateDto data) => await service.Add(data);

        [HttpDelete]
        public async Task Delete(ID<UserDto> userID) => await service.Delete(userID);

        [HttpPut]
        public async Task Update(UserDto user) => await service.Update(user);

        [HttpGet]
        [Route("verifypassword")]
        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password) => await service.VerifyPassword(userID, password);

        [HttpPut]
        [Route("password")]
        public async Task UpdatePassword(ID<UserDto> userID, string newPass) => await service.UpdatePassword(userID, newPass);

        [HttpGet]
        [Route("id")]
        public async Task<UserDto> Find(ID<UserDto> userID) => await service.Find(userID);

        [HttpGet]
        [Route("login")]
        public async Task<UserDto> Find(string login) => await service.Find(login);

        [HttpGet]
        [Route("exists/id")]
        public async Task<bool> Exists(ID<UserDto> userID) => await service.Exists(userID);
        
        [HttpGet]
        [Route("exists/login")]
        public async Task<bool> Exists(string login) => await service.Exists(login);

        //public async Task<User> GetCurrentUser() => await userService.GetCurrentUser();
    }
}
