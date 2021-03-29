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
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Update(ItemDto item);

        /// <summary>
        /// Finds item in db.
        /// </summary>
        /// <param name="itemID">Id of item to find.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ItemDto> Find(ID<ItemDto> itemID);

        /// <summary>
        /// Gets list of items that belongs to that project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID);

        /// <summary>
        /// Gets list of items that belongs to that objective.
        /// </summary>
        /// <param name="objectiveID">Objective's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Download files from remote connection to local storage.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds);

        /// <summary>
        /// Delete items from remote connection.
        /// </summary>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds);
    }
}
