﻿using MRS.DocumentManagement.Interface.SyncData;
using System.Threading.Tasks;

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
        Task<(bool, string)> Connect(dynamic param);

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
        ///
        // TODO uncomment after yandex PR with syncronization functionality will be merged
        Task<ProgressSync> GetProgressSyncronization();
    }
}
