﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface.Services
{
    public interface IAuthorizationService
    {
        /// <summary>
        /// Get all registered roles
        /// </summary>
        /// <returns>Empty enumerable if no roles were registered</returns>
        Task<IEnumerable<string>> GetAllRoles();
        /// <summary>
        /// Add role to user
        /// </summary>
        /// <returns>True if role is added, false if user already had this role</returns>
        Task<bool> AddRole(ID<User> userID, string role);
        /// <summary>
        /// Remove role from user. If this role is not referenced anymore it's deleted.
        /// </summary>
        /// <returns>True if role was removed, false if user hadn't this role</returns>
        Task<bool> RemoveRole(ID<User> userID, string role);
        /// <summary>
        /// Get all roles of specified user
        /// </summary>
        Task<IEnumerable<string>> GetUserRoles(ID<User> userID);
        /// <summary>
        /// Check if user is in role
        /// </summary>
        Task<bool> IsInRole(ID<User> userID, string role);
    }
}
