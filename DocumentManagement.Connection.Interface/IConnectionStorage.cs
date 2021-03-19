using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface
{
    /// <summary>
    /// Interface for working with remote storage.
    /// </summary>
    public interface IConnectionStorage
    {
        /// <summary>
        /// Download files from remote storage.
        /// </summary>
        /// <param name="projectId">Id of the project to download from.</param>
        /// <param name="itemExternalDto">Items to download.</param>
        /// <returns>Download result.</returns>
        Task<bool> DownloadFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDto);

        /// <summary>
        /// Deletes files from remote storage.
        /// </summary>
        /// <param name="itemExternalDto">Items to delete.</param>
        /// <returns>Deletion result.</returns>
        Task<bool> DeleteFiles(IEnumerable<ItemExternalDto> itemExternalDto);
    }
}
