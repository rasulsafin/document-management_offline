﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service to manage ConnectionTypes.
    /// </summary>
    public interface IConnectionTypeService
    {
        /// <summary>
        /// Add new connection type with given name.
        /// </summary>
        /// <param name="typeName">Name of the new connection type.</param>
        /// <returns>ID of added connection type.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ArgumentValidationException">Thrown when connection type with type name already exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ID<ConnectionTypeDto>> Add(string typeName);

        /// <summary>
        /// Find a connection type by ID.
        /// </summary>
        /// <param name="id">Type's ID.</param>
        /// <returns>Searching connection type.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when connection type does not exist.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ConnectionTypeDto> Find(ID<ConnectionTypeDto> id);

        /// <summary>
        /// Find a connection type by name.
        /// </summary>
        /// <param name="name">Type's name.</param>
        /// <returns>Searching connection type.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when connection type with that name does not exist.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ConnectionTypeDto> Find(string name);

        /// <summary>
        /// Get all registered connection types.
        /// </summary>
        /// <returns>Collection of connection types.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something went wrong.</exception>
        Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes();

        /// <summary>
        /// Remove the connection type.
        /// </summary>
        /// <param name="id">ID of the type to remove.</param>
        /// <returns>Removing result.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when connection type does not exist.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Remove(ID<ConnectionTypeDto> id);

        /// <summary>
        /// Register all existing Connection Types. Use by admin once at the start OR when new connection types are added.
        /// </summary>
        /// <returns>Result of registration.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something went wrong.</exception>
        Task<bool> RegisterAll();

    }
}