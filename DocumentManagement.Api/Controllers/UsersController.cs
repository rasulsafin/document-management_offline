using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [Route("Get/Users")]
        public async Task<IEnumerable<User>> GetAllUsers() => await service.GetAllUsers();

        [HttpPost]
        [Route("Add/User")]
        public async Task<ID<User>> Add(UserToCreate data) => await service.Add(data);

        [HttpDelete]
        [Route("Delete/User")]
        public async Task Delete(ID<User> userID) => await service.Delete(userID);

        [HttpPut]
        [Route("Update/User")]
        public async Task Update(User user) => await service.Update(user);

        [HttpGet]
        [Route("Verify/Password")]
        public async Task<bool> VerifyPassword(ID<User> userID, string password) => await service.VerifyPassword(userID, password);

        [HttpPut]
        [Route("Update/Password")]
        public async Task UpdatePassword(ID<User> userID, string newPass) => await service.UpdatePassword(userID, newPass);

        [HttpGet]
        [Route("Find/User")]
        public async Task<User> Find(ID<User> userID) => await service.Find(userID);

        [HttpGet]
        [Route("Find/User/Login")]
        public async Task<User> Find(string login) => await service.Find(login);

        [HttpGet]
        [Route("Exists/User")]
        public async Task<bool> Exists(ID<User> userID) => await service.Exists(userID);
        
        [HttpGet]
        [Route("Exists/Login")]
        public async Task<bool> Exists(string login) => await service.Exists(login);

        //public async Task<User> GetCurrentUser() => await userService.GetCurrentUser();
    }
}
