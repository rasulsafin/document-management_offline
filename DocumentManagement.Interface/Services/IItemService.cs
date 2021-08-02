using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for managing files/items.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Updates item.
        /// </summary>
        /// <param name="item">Data to update.</param>
        /// <returns>True if updated.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when items does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Update(ItemDto item);

        /// <summary>
        /// Finds item in db.
        /// </summary>
        /// <param name="itemID">Id of item to find.</param>
        /// <returns>Found item.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when item with that id does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ItemDto> Find(ID<ItemDto> itemID);

        /// <summary>
        /// Gets list of items that belongs to that project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Collection of items.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when project with that id does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID);

        /// <summary>
        /// Gets list of items that belongs to that objective.
        /// </summary>
        /// <param name="objectiveID">Objective's id.</param>
        /// <returns>Collection of items.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.ANotFoundException">Thrown when objective with that id does not exists.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Download files from remote connection to local storage.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>Id of the created long request.</returns>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something went wrong.</exception>
        Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds);

        /// <summary>
        /// Delete items from remote connection.
        /// </summary>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>True if deleted successfully.</returns>
        /// <exception cref="System.NotImplementedException">Thrown while method is not implemented.</exception>
        /// <exception cref="MRS.DocumentManagement.Interface.Exceptions.DocumentManagementException">Thrown when something went wrong.</exception>
        Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds);
    }
}
