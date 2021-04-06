using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for managing authorization of users.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Get all registered roles.
        /// </summary>
        /// <returns>Empty enumerable if no roles were registered.</returns>
        Task<IEnumerable<string>> GetAllRoles();

        /// <summary>
        /// Add role to user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to add.</param>
        /// <returns>True if role is added.</returns>
        Task<bool> AddRole(ID<UserDto> userID, string role);

        /// <summary>
        /// Remove role from user. If this role is not referenced anymore it's deleted.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to remove.</param>
        /// <returns>True if role was removed.</returns>
        Task<bool> RemoveRole(ID<UserDto> userID, string role);

        /// <summary>
        /// Get all roles of specified user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Collection of roles.</returns>
        Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID);

        /// <summary>
        /// Check if user is in role.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to check.</param>
        /// <returns>True if user is in role, false otherwise.</returns>
        Task<bool> IsInRole(ID<UserDto> userID, string role);

        /// <summary>
        /// Login the user if credentials are correct.
        /// </summary>
        /// <param name="username">User's login.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Validated user.</returns>
        Task<ValidatedUserDto> Login(string username, string password);
    }
}
