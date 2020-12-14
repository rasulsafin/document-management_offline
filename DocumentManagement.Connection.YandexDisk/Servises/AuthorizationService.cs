using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Connection.YandexDisk
{
    public class AuthorizationService : IAuthorizationService
    {
        public Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInRole(ID<UserDto> userID, string role)
        {
            throw new NotImplementedException();
        }

        public Task<ValidatedUserDto> Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            throw new NotImplementedException();
        }
    }
}
