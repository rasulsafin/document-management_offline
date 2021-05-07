using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for managing User entities.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get all registered users.
        /// </summary>
        /// <returns>A IEnumerable of <see cref="UserDto"/> representing the list of users.</returns>
        Task<IEnumerable<UserDto>> GetAllUsers();

        /// <summary>
        /// Add new user.
        /// </summary>
        /// <param name="data">New user data.</param>
        /// <exception cref="InvalidDataException">Throws if passed data fails database validation checks
        /// (e.g. user with current login already exists).</exception>
        /// <returns>Identifier of new user.</returns>
        Task<ID<UserDto>> Add(UserToCreateDto data);

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="userID">User ID to be deleted.</param>
        /// <returns>True, if user was deleted.</returns>
        /// <exception cref="ArgumentException">Throws if user with passed ID does not exists.</exception>
        Task<bool> Delete(ID<UserDto> userID);

        /// <summary>
        /// Update user data.
        /// </summary>
        /// <param name="user">User data.</param>
        /// <returns>True, if user was updated.</returns>
        /// <exception cref="ArgumentException">Throws if user with passed ID does not exists.</exception>
        Task<bool> Update(UserDto user);

        /// <summary>
        /// Check if password valid for specified user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="password">Password to verify.</param>
        /// <returns>True if password was verified successfully.</returns>
        /// <exception cref="InvalidDataException">Throws if password is wrong.</exception>
        /// <exception cref="ANotFoundException">Throws if user with passed ID does not exists.</exception>
        Task<bool> VerifyPassword(ID<UserDto> userID, string password);

        /// <summary>
        /// Set new password for specified user.
        /// </summary>
        /// <returns>True if password was successfully updated.</returns>
        /// <param name="userID">User's ID.</param>
        /// <param name="newPass">New password.</param>
        /// <exception cref="ANotFoundException">Throws if user with passed ID does not exists.</exception>
        Task<bool> UpdatePassword(ID<UserDto> userID, string newPass);

        /// <summary>
        /// Query user data by user ID.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Found User.</returns>
        /// <exception cref="ANotFoundException">Throws if user with passed ID does not exists.</exception>
        Task<UserDto> Find(ID<UserDto> userID);

        /// <summary>
        /// Query user data by user login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>Null if user with specified login was not found</returns>
        /// <exception cref="ANotFoundException">Throws if user with passed login does not exists.</exception>
        Task<UserDto> Find(string login);

        /// <summary>
        /// Check if user with specified ID exists.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>True if user exists, returns false otherwise.</returns>
        Task<bool> Exists(ID<UserDto> userID);

        /// <summary>
        /// Check if user with specified login exists.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>True if user exists, returns false otherwise.</returns>
        Task<bool> Exists(string login);
    }
}
