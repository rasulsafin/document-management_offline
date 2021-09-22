using Brio.Docs.Database;
using Brio.Docs.Synchronization.Models;
using System.Threading.Tasks;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents methods to bind to a parent object.
    /// </summary>
    /// <typeparam name="TDB">Class that would be linked.</typeparam>
    internal interface ILinker<in TDB>
    {
        /// <summary>
        /// Create link from the object to a parent object.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="element">The attached object.</param>
        /// <param name="parent">The parent object.</param>
        /// <param name="entityType">The type of attached and parent objects.</param>
        /// <returns>The task of this action.</returns>
        Task Link(DMContext context, TDB element, object parent, EntityType entityType);

        /// <summary>
        /// Remove link from the object to a parent object.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="element">The attached object.</param>
        /// <param name="parent">The parent object.</param>
        /// <param name="entityType">The type of attached and parent objects.</param>
        /// <returns>The task of this action.</returns>
        Task Unlink(DMContext context, TDB element, object parent, EntityType entityType);

        /// <summary>
        /// Update a needed info of the object or the parent.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="element">The attached object.</param>
        /// <param name="parent">The parent object.</param>
        /// <param name="entityType">The type of attached and parent objects.</param>
        /// <returns>The task of this action.</returns>
        Task Update(DMContext context, TDB element, object parent, EntityType entityType);
    }
}
