using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;
using System;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service to manage RemoteConnections (e.g. YandexDisk, TDMS, BIM360).
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Add new ConnectionInfo and link it to User.
        /// </summary>
        /// <param name="connectionInfo">ConnectionInfo to create.</param>
        /// <returns>True if ConnectionInfo was successfully created.</returns>
        Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto connectionInfo);

        /// <summary>
        /// Get ConnectionInfo for the specific user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>ConnectionInfoDto.</returns>
        Task<ConnectionInfoDto> Get(ID<UserDto> userID);

        /// <summary>
        /// Connect user to Remote connection(e.g. YandexDisk, TDMS, BIM360), using user's ConnectionInfo.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        Task<RequestID> Connect(ID<UserDto> userID);

        /// <summary>
        /// Get current status of user's connection.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Status of the connection.</returns>
        Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID);

        /// <summary>
        /// Get available to user enumeration values of enum type.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="enumerationTypeID">Enumeration Type's ID.</param>
        /// <returns>Collection of enumeration values.</returns>
        Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID);

        /// <summary>
        /// Synchronize user's data.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        Task<RequestID> Synchronize(ID<UserDto> userID);

        /// <summary>
        /// Gets the dates of synchronizations for the user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>The date of the last synchronization or null if user is not synchronized.</returns>
        Task<IEnumerable<DateTime>> GetSynchronizationsDates(ID<UserDto> userID);

        /// <summary>
        /// Removes the last synchronization date of the user for an attempt to sync entities that were updated earlier than the last sync date.
        /// The entities will not be returned to the previous state.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>True, the last synchronization date is removed.</returns>
        Task<bool> RemoveLastSynchronizationDate(ID<UserDto> userID);
    }
}
