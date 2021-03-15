using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service to manage RemoteConnections (e.g. YandexDisk, TDMS, BIM360).
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Adds new ConnectionInfo and links it to User.
        /// </summary>
        /// <param name="connectionInfo">ConnectionInfo to create.</param>
        /// <returns>True if ConnectionInfo was succesfuly created.</returns>
        Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto connectionInfo);

        /// <summary>
        /// Gets ConnectionInfo for the specific user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>ConnectionInfoDto.</returns>
        Task<ConnectionInfoDto> Get(ID<UserDto> userID);

        /// <summary>
        /// Connects user to Remote connection(e.g. YandexDisk, TDMS, BIM360), using user's ConnectionInfo.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Status of the connection.</returns>
        Task<ConnectionStatusDto> Connect(ID<UserDto> userID);

        /// <summary>
        /// Gets current stutus of user's connetion.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Status of the connection.</returns>
        Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID);

        /// <summary>
        /// Gets available to user enumeration values of enum type.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="enumerationTypeID">Enumeration Type's ID.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID);

        /// <summary>
        /// Synchronize user's data.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Result of the synchronization.</returns>
        Task<string> Synchronize(ID<UserDto> userID);

        /// <summary>
        /// Gets status of synchronization.
        /// </summary>
        /// <param name="synchronizationID">Synchronization's id.</param>
        /// <returns>True if synchronization is completed.</returns>
        Task<bool> IsSynchronizationComplete(string synchronizationID);

        /// <summary>
        /// Gets synchronization result.
        /// </summary>
        /// <param name="synchronizationID">Synchronization's id.</param>
        /// <returns>True if synchronization is completed without errors.</returns>
        Task<bool> GetSynchronizationResult(string synchronizationID);
    }
}
