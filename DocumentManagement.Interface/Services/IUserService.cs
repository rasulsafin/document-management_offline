using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Get all registered users
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllUsers();
        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="data">New user data</param>
        /// <exception cref="InvalidDataException">Throws if passed data fails database validation checks 
        /// (e.g. user with current login already exists)</exception>
        /// <returns>Identifier of new user</returns>
        Task<ID<UserDto>> Add(UserToCreateDto data);
        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userID">User ID to be deleted</param>
        /// <returns>True, if user was deleted. False if user was not found.</returns>
        Task<bool> Delete(ID<UserDto> userID);
        /// <summary>
        /// Update user data
        /// </summary>
        Task<bool> Update(UserDto user);
        /// <summary>
        /// Check if password valid for specified user
        /// </summary>
        Task<bool> VerifyPassword(ID<UserDto> userID, string password);
        /// <summary>
        /// Set new password for specified user
        /// </summary>
        Task<bool> UpdatePassword(ID<UserDto> userID, string newPass);
        /// <summary>
        /// Query user data by user ID
        /// </summary>
        /// <returns>Null if user with specified login was not found</returns>
        Task<UserDto> Find(ID<UserDto> userID);
        /// <summary>
        /// Query user data by user login
        /// </summary>
        /// <returns>Null if user with specified login was not found</returns>
        Task<UserDto> Find(string login);
        /// <summary>
        /// Check if user with specified ID exists
        /// </summary>
        Task<bool> Exists(ID<UserDto> userID);
        /// <summary>
        /// Check if user with specified login exists
        /// </summary>
        Task<bool> Exists(string login);
    }
}
