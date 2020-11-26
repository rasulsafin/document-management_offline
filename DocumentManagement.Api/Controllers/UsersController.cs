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
        public async Task<IEnumerable<User>> GetAllUsers() => await service.GetAllUsers();

        [HttpPost]
        public async Task<ID<User>> Add(UserToCreate data) => await service.Add(data);

        [HttpDelete]
        public async Task Delete(ID<User> userID) => await service.Delete(userID);

        [HttpPut]
        public async Task Update(User user) => await service.Update(user);

        [HttpGet]
        public async Task<bool> VerifyPassword(ID<User> userID, string password) => await service.VerifyPassword(userID, password);

        [HttpPut]
        public async Task UpdatePassword(ID<User> userID, string newPass) => await service.UpdatePassword(userID, newPass);

        [HttpGet]
        public async Task<User> Find(ID<User> userID) => await service.Find(userID);

        [HttpGet]
        public async Task<User> Find(string login) => await service.Find(login);

        [HttpGet]
        public async Task<bool> Exists(ID<User> userID) => await service.Exists(userID);
        
        [HttpGet]
        public async Task<bool> Exists(string login) => await service.Exists(login);

        //public async Task<User> GetCurrentUser() => await userService.GetCurrentUser();
    }
}
