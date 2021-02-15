using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizer
{
    /// <summary>
    /// The synchronizer is a single entity
    /// </summary>
    public interface ISynchroTable
    {
        /// <summary>
        /// Get a list of revisions.
        /// </summary>
        /// <param name="revisions">
        /// The complex variable from which to extract the collection.
        /// </param>
        /// <returns> list of revisions </returns>
        List<Revision> GetRevisions(RevisionCollection revisions);

        /// <summary>
        /// Install the updated revision.
        /// </summary>
        /// <param name="revisions">
        /// The complex variable that you want to write the new revision to.
        /// </param>
        /// <param name="rev"> revision </param>
        void SetRevision(RevisionCollection revisions, Revision rev);

        /// <summary>
        /// Download an object from the server
        /// </summary>
        /// <param name="action">Information about the action</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task Download(SyncAction action);

        /// <summary>
        /// Upload an object to the server
        /// </summary>
        /// <param name="action">Information about the action</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task Upload(SyncAction action);

        /// <summary>
        /// Delete an object from the local collection
        /// </summary>
        /// <param name="action">Information about the action</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteLocal(SyncAction action);

        /// <summary>
        /// Delete an object from a remote collection
        /// </summary>
        /// <param name="action">Information about the action</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteRemote(SyncAction action);

        /// <summary>
        /// Add records that are in the database but have not yet been tracked (A function for copying databases)
        /// TODO: Permanent it is not necessary to think about disabling it(enable it on demand)
        /// </summary>
        /// <param name="local">The complex variable that you want to write the new revision to.</param>
        void CheckDBRevision(RevisionCollection local);
    }
}
