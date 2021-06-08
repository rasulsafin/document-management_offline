using System;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    /// <summary>
    /// Capttures state of BIM360.
    /// </summary>
    public interface IBim360SnapshotFiller
    {
        /// <summary>
        /// Sets need to include test projects & issues to snapshot.
        /// </summary>
        bool IgnoreTestEntities { set; }

        /// <summary>
        /// Capture hubs.
        /// </summary>
        /// <returns>The task of the operation.</returns>
        Task UpdateHubsIfNull();

        /// <summary>
        /// Capture projects.
        /// </summary>
        /// <returns>The task of the operation.</returns>
        Task UpdateProjectsIfNull();

        /// <summary>
        /// Capture issues.
        /// </summary>
        /// <param name="date">The snapshot will be capture for issues updated after this date.</param>
        /// <returns>The task of the operation.</returns>
        Task UpdateIssuesIfNull(DateTime date = default);
    }
}
