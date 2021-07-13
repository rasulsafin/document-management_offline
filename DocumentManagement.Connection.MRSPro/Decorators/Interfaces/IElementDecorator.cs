using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    /// <summary>
    /// Service for MrsPro.Elements.
    /// </summary>
    public interface IElementDecorator
    {
        /// <summary>
        /// Get Enumerable of all elements, that were created or modified after a certain date.
        /// </summary>
        /// <param name="date">Date.</param>
        /// <returns>Enumerable of elements.</returns>
        Task<IEnumerable<IElement>> GetAll(DateTime date);

        /// <summary>
        /// Get element by its id.
        /// </summary>
        /// <param name="id">Element's id.</param>
        /// <returns>Element.</returns>
        Task<IElement> GetElementById(string id);

        /// <summary>
        /// Get list of elements by their ids.
        /// </summary>
        /// <param name="ids">List of ids.</param>
        /// <returns>List of elements.</returns>
        Task<IEnumerable<IElement>> GetElementsByIds(IReadOnlyCollection<string> ids);

        /// <summary>
        /// Creates new element.
        /// </summary>
        /// <param name="element">Element to create.</param>
        /// <returns>Created element with the new id.</returns>
        Task<IElement> PostElement(IElement element);

        /// <summary>
        /// Patch Element's values, marked as IsPatchable. 
        /// </summary>
        /// <param name="valuesToPatch">Id of the patched entity and list of patches.</param>
        /// <returns>Patched element.</returns>
        Task<IElement> PatchElement(UpdatedValues valuesToPatch);

        /// <summary>
        /// Delete element by its id.
        /// </summary>
        /// <param name="id">Element id.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        Task<bool> DeleteElementById(string id);
    }
}
