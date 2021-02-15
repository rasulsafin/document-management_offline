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

        #region TODO

        // TODO: Syncronization!
        // Task<bool> StartSyncronization(ID<UserDto> userID);
        // Task<bool> StopSyncronization(ID<UserDto> userID);
        // Task<?> GetProgressSyncronization(ConnectionInfoDto info);
        //
        // OR
        //
        // Task<bool> Sync(System.IProgress<SyncData.ProgressSync> prog, System.Threading.CancellationToken token);

        // TODO: In case user can have several connections?
        // Task<ConnectionInfoDto> GetCurrentConnection(ID<UserDto> userId);

        // TODO: If we need different method than connect?
        // Task<bool> Reconnect(ConnectionInfoToCreateDto connectionInfo);

        // TODO: Update connectionInfo from user's request?
        // Task<bool> Update(ConnectionInfoDto connectionInfo);
        #endregion
    }
}
