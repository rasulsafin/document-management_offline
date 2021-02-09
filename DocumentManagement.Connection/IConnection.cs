using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

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
        /// <param name="param">Dynamic params.</param>
        /// <returns>Result success and additional result data.</returns>
        Task<ConnectionStatusDto> Connect(dynamic param);

        /// <summary>
        /// Checks if the saved authorization data is correct.
        /// </summary>
        /// <returns>Result of check.</returns>
        Task<bool> IsAuthDataCorrect();

        /// <summary>
        /// Starts syncronization between local and remote DMs.
        /// </summary>
        /// <returns>Result of syncronizing start process.</returns>
        Task<bool> StartSyncronization();

        /// <summary>
        /// Stops syncronization between local and remote DMs.
        /// </summary>
        /// <returns>Result of syncronizing stop process.</returns>
        Task StopSyncronization();

        /// <summary>
        /// Gets current syncronization progress.
        /// </summary>
        /// <returns>Progress.</returns>
        Task<ProgressSync> GetProgressSyncronization();
    }
}
