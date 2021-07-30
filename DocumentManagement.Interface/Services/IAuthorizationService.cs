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
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something went wrong.</exception>
        Task<IEnumerable<string>> GetAllRoles();

        /// <summary>
        /// Add role to user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to add.</param>
        /// <returns>True if role is added.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ArgumentValidationException">Thrown when user is already in role.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> AddRole(ID<UserDto> userID, string role);

        /// <summary>
        /// Remove role from user. If this role is not referenced anymore it's deleted.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to remove.</param>
        /// <returns>True if role was removed.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ArgumentValidationException">Thrown when user do not have the role.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> RemoveRole(ID<UserDto> userID, string role);

        /// <summary>
        /// Get all roles of specified user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Collection of roles.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID);

        /// <summary>
        /// Check if user is in role.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to check.</param>
        /// <returns>True if user is in role, false otherwise.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> IsInRole(ID<UserDto> userID, string role);

        /// <summary>
        /// Login the user if credentials are correct.
        /// </summary>
        /// <param name="username">User's login.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Validated user.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ArgumentValidationException">Thrown when user login OR password is invalid.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ValidatedUserDto> Login(string username, string password);
    }
}
