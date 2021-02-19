using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Interface
{
    /// <summary>
    /// Interface for any type of connection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Connect to remote DM.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <returns>Result success and additional result data.</returns>
        Task<ConnectionStatusDto> Connect(ConnectionInfoDto info);

        /// <summary>
        /// Checks if the saved authorization data is correct.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <returns>Result of check.</returns>
        Task<bool> IsAuthDataCorrect(ConnectionInfoDto info);

        /// <summary>
        /// Current status of the connection.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info);

        /// <summary>
        /// Method fills all the relevant fields in ConnectionInfo
        /// e.g. AuthFieldValues and EnumerationTypes in order to link them to User.
        /// </summary>
        /// <param name="info">ConnectionInfoDto to fill in.</param>
        /// <returns>Filed ConnectionInfoDto.</returns>
        Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info);

        /// <summary>
        /// Get the information about the current Connection.
        /// </summary>
        /// <returns>Filed ConnectionTypeDto.</returns>
        ConnectionTypeDto GetConnectionType();

        #region TODO

        // TODO: Syncronization!
        /// <summary>
        /// Starts syncronization between local and remote DMs.
        /// </summary>
        /// <returns>Result of syncronizing start process.</returns>
        // Task<bool> StartSyncronization();

        /// <summary>
        /// Stops syncronization between local and remote DMs.
        /// </summary>
        /// <returns>Result of syncronizing stop process.</returns>
        // Task<bool> StopSyncronization();

        /// <summary>
        /// Gets current syncronization progress.
        /// </summary>
        /// <returns>Progress.</returns>
        // Task<ProgressSync> GetProgressSyncronization();

        #endregion
    }
}
