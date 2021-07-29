using System;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    /// <summary>
    /// Captures state of BIM360.
    /// </summary>
    public interface IBim360SnapshotFiller
    {
        /// <summary>
        /// Sets need to include test projects & issues to snapshot.
        /// </summary>
        bool IgnoreTestEntities { set; }

        /// <summary>
        /// Captures hubs.
        /// </summary>
        /// <returns>The task of the operation.</returns>
        Task UpdateHubsIfNull();

        /// <summary>
        /// Captures projects.
        /// </summary>
        /// <returns>The task of the operation.</returns>
        Task UpdateProjectsIfNull();

        /// <summary>
        /// Captures issues.
        /// </summary>
        /// <param name="date">The snapshot will be capture for issues updated after this date.</param>
        /// <returns>The task of the operation.</returns>
        Task UpdateIssuesIfNull(DateTime date = default);

        /// <summary>
        /// Captures issue types.
        /// </summary>
        /// <returns>The task of the operation.</returns>
        Task UpdateIssueTypes();
    }
}
