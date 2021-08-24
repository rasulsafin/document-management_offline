using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    /// <summary>
    /// Represents a class for working with BIM 360 properties as dynamic field.
    /// </summary>
    /// <typeparam name="T">The type that can uniquely represent a given variant of BIM 360 dependent properties.</typeparam>
    /// <typeparam name="TSnapshot">The type of snaphot for given type.</typeparam>
    /// <typeparam name="TVariantID">The identifier to find needed variant.</typeparam>
    internal interface IEnumCreator<T, TSnapshot, out TVariantID>
        where TSnapshot : AEnumVariantSnapshot<T>
    {
        /// <summary>
        /// Represents the identifier for current enum.
        /// </summary>
        string EnumExternalID { get; }

        /// <summary>
        /// Represents the display name of this property.
        /// </summary>
        string EnumDisplayName { get; }

        /// <summary>
        /// Gets whether the field can contain null value.
        /// </summary>
        bool CanBeNull { get; }

        /// <summary>
        /// Gets identifier for a null value for the current field.
        /// </summary>
        string NullID { get; }

        /// <summary>
        /// Gets ordered identifiers of this variants.
        /// </summary>
        /// <param name="variants">Variants required to get identifiers.</param>
        /// <returns>Ordered identifiers of this variants.</returns>
        IOrderedEnumerable<TVariantID> GetOrderedIDs(IEnumerable<TSnapshot> variants);

        /// <summary>
        /// Gets display name of this variant.
        /// </summary>
        /// <param name="variant">The variant of current enum.</param>
        /// <returns>The display name of this variant.</returns>
        string GetVariantDisplayName(TSnapshot variant);

        /// <summary>
        /// Sends request for getting variants of the current enum.
        /// </summary>
        /// <param name="issuesService">Issues Service for Forge requests.</param>
        /// <param name="projectSnapshot">Projects Service for Forge requests.</param>
        /// <returns>All variants of the current enum for this user.</returns>
        Task<IEnumerable<TSnapshot>> GetVariantsFromRemote(
            IssuesService issuesService,
            ProjectSnapshot projectSnapshot);

        /// <summary>
        /// Gets snapshots of variants from a given project snapshot.
        /// </summary>
        /// <param name="projects">The source snapshot of project.</param>
        /// <returns>The enumeration of the snapshots of this enum.</returns>
        IEnumerable<TSnapshot> GetSnapshots(IEnumerable<ProjectSnapshot> projects);

        /// <summary>
        /// Deserializes a stored identifier with multiple variant IDs.
        /// </summary>
        /// <param name="externalID">The stored identifier with multiple variant IDs.</param>
        /// <returns>Multiple variant IDs.</returns>
        IEnumerable<TVariantID> DeserializeID(string externalID);
    }
}
