using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for managing Objective Types.
    /// </summary>
    public interface IObjectiveTypeService
    {
        /// <summary>
        /// Get list of objective types accessible to specific Connection Type.
        /// </summary>
        /// <param name="id">Connection type id.</param>
        /// <returns>Objective Type.</returns>
        Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<ConnectionTypeDto> id);

        /// <summary>
        /// Add new objective type.
        /// </summary>
        /// <param name="typeName">Name.</param>
        /// <returns>Id of created objective type.</returns>
        Task<ID<ObjectiveTypeDto>> Add(string typeName);

        /// <summary>
        /// Delete objective type by id.
        /// </summary>
        /// <param name="id">Objective Type's id.</param>
        /// <returns>True, if deletion was successful.</returns>
        Task<bool> Remove(ID<ObjectiveTypeDto> id);

        /// <summary>
        /// Get Objective Type by id.
        /// </summary>
        /// <param name="id">Objective Type's id.</param>
        /// <returns>Found type.</returns>
        Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id);

        /// <summary>
        /// Get Objective Type by name.
        /// </summary>
        /// <param name="typename">Name of type.</param>
        /// <returns>Found type.</returns>
        Task<ObjectiveTypeDto> Find(string typename);
    }
}
