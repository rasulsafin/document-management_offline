using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Synchronization.Interfaces
{
    /// <summary>
    /// Represents methods to upload and remove items.
    /// </summary>
    internal interface IItemsUpdater
    {
        /// <summary>
        /// Uploads new item & save data to snapshot.
        /// </summary>
        /// <param name="project">The project that will contain the item.</param>
        /// <param name="fullPath">The full path of the item.</param>
        /// <returns>The snapshot of the result item.</returns>
        Task<ItemSnapshot> PostItem(ProjectSnapshot project, string fullPath);

        /// <summary>
        /// Uploads new item as a new version of an existing item & save data to snapshot.
        /// </summary>
        /// <param name="project">The project containing the item.</param>
        /// <param name="itemID">The ID of the existing item.</param>
        /// <param name="fullPath">The full path of the uploading item.</param>
        /// <returns>The snapshot of the result item.</returns>
        Task<ItemSnapshot> UpdateVersion(ProjectSnapshot project, string itemID, string fullPath);

        /// <summary>
        /// Removes item from a project.
        /// </summary>
        /// <param name="project">The project containing the item.</param>
        /// <param name="itemID">The ID of the existing item.</param>
        /// <returns>The snapshot of the removed item.</returns>
        Task Remove(ProjectSnapshot project, string itemID);
    }
}
