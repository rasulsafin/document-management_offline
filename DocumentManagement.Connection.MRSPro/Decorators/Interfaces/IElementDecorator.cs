using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
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
        Task<IEnumerable<IElementObject>> GetAll(DateTime date);

        /// <summary>
        /// Get element by its id.
        /// </summary>
        /// <param name="id">Element's id.</param>
        /// <returns>Element.</returns>
        Task<IElementObject> GetElementById(string id);

        /// <summary>
        /// Get list of elements by their ids.
        /// </summary>
        /// <param name="ids">List of ids.</param>
        /// <returns>List of elements.</returns>
        Task<IEnumerable<IElementObject>> GetElementsByIds(IReadOnlyCollection<string> ids);

        /// <summary>
        /// Creates new element.
        /// </summary>
        /// <param name="element">Element to create.</param>
        /// <returns>Created element with the new id.</returns>
        Task<IElementObject> PostElement(IElementObject element);

        /// <summary>
        /// Patch Element's values, marked as IsPatchable. 
        /// </summary>
        /// <param name="valuesToPatch">Id of the patched entity and list of patches.</param>
        /// <returns>Patched element.</returns>
        Task<IElementObject> PatchElement(UpdatedValues valuesToPatch);

        /// <summary>
        /// Delete element by its id.
        /// </summary>
        /// <param name="id">Element id.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        Task<bool> DeleteElementById(string id);

        /// <summary>
        /// Get files attached to an element by element's id.
        /// </summary>
        /// <param name="id">Element's id.</param>
        /// <returns>Enumerator for attached files.</returns>
        Task<IEnumerable<IElementAttachment>> GetAttachments(string id);
    }
}
