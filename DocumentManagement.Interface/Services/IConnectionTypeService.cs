using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service to manage ConnectionTypes.
    /// </summary>
    public interface IConnectionTypeService
    {
        /// <summary>
        /// Adds new connection type with given name.
        /// </summary>
        /// <param name="typeName">Name of the newconnection type.</param>
        /// <returns>ID of added connection type.</returns>
        Task<ID<ConnectionTypeDto>> Add(string typeName);

        /// <summary>
        /// Finds a connection type by ID.
        /// </summary>
        /// <param name="id">Type's ID</param>
        /// <returns>Searching connection type.</returns>
        Task<ConnectionTypeDto> Find(ID<ConnectionTypeDto> id);

        /// <summary>
        /// Finds a connection type by name.
        /// </summary>
        /// <param name="name">Type's name</param>
        /// <returns>Searching connection type.</returns>
        Task<ConnectionTypeDto> Find(string name);

        /// <summary>
        /// Gets all registered connection types.
        /// </summary>
        /// <returns>Collection of connection types.</returns>
        Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes();

        /// <summary>
        /// Removes the connection type
        /// </summary>
        /// <param name="id">ID of the type to remove.</param>
        /// <returns>Removing result.</returns>
        Task<bool> Remove(ID<ConnectionTypeDto> id);
    }
}