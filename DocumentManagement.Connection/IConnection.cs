using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    /// <summary>
    /// Interface for any type of connection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Connect to remote DM.
        /// </summary>
        /// <param name="info">Information abot the connection.</param>
        /// <returns>Result success and additional result data.</returns>
        Task<ConnectionStatusDto> Connect(ConnectionInfoDto info);

        /// <summary>
        /// Checks if the saved authorization data is correct.
        /// </summary>
        /// <returns>Result of check.</returns>
        Task<bool> IsAuthDataCorrect();

        /// <summary>
        /// Current status of the connection.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ConnectionStatusDto> GetStatus();

        // TODO: Syncronization!
        ///// <summary>
        ///// Starts syncronization between local and remote DMs.
        ///// </summary>
        ///// <returns>Result of syncronizing start process.</returns>
        // Task<bool> StartSyncronization();

        ///// <summary>
        ///// Stops syncronization between local and remote DMs.
        ///// </summary>
        ///// <returns>Result of syncronizing stop process.</returns>
        // Task<bool> StopSyncronization();

        /// <summary>
        /// Gets current syncronization progress.
        /// </summary>
        /// <returns>Progress.</returns>
        // Task<ProgressSync> GetProgressSyncronization();

    }
}
