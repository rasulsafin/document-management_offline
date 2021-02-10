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
        Task<bool> Add(ConnectionInfoToCreateDto connectionInfo);

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
        /// <returns>Status of the connection.</returns>
        Task<ConnectionStatusDto> GetRemoteConnectionStatus();

        #region TODO

        // TODO: Make Progress using IProgress and do it in one method
        ///// <summary>
        ///// Starts syncronization between database and remote connetion.
        ///// </summary>
        ///// <param name="userID">User's ID.</param>
        ///// <returns>True if syncronization started.</returns>
        // Task<bool> StartSyncronization(ID<UserDto> userID);

        // Task<bool> Sync(System.IProgress<SyncData.ProgressSync> prog, System.Threading.CancellationToken token);

        ///// <summary>
        ///// Stops syncronization between database and remote connetion.
        ///// </summary>
        ///// <param name="userID">User's ID.</param>
        ///// <returns>True if syncronization successfully stopped.</returns>
        // Task<bool> StopSyncronization(ID<UserDto> userID);

        // In case user can have several connections?
        // Task<ConnectionInfoDto> GetCurrentConnection(ID<UserDto> userId);

        // If we need different method than connect?
        // Task<bool> Reconnect(ConnectionInfoToCreateDto connectionInfo);

        // Task<IEnumerable<EnumVariantDto>> GetEnumVariants(string dynamicFieldKey);

        // Task<?> GetProgressSyncronization(ConnectionInfoDto info);
        #endregion
    }
}
