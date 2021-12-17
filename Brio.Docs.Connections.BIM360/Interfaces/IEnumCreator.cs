using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;

namespace Brio.Docs.Connections.Bim360.Interfaces
{
    /// <summary>
    /// Represents a class for working with BIM 360 properties as dynamic field.
    /// </summary>
    /// <typeparam name="T">The type that can uniquely represent a given variant of BIM 360 dependent properties.</typeparam>
    /// <typeparam name="TSnapshot">The type of snapshot for given type.</typeparam>
    /// <typeparam name="TVariantID">The identifier to find needed variant.</typeparam>
    internal interface IEnumCreator<T, TSnapshot, out TVariantID> : IEnumIdentification<TSnapshot>
        where TSnapshot : AEnumVariantSnapshot<T>
    {
        /// <summary>
        /// Gets ordered identifiers of this variants.
        /// </summary>
        /// <param name="variants">Variants required to get identifiers.</param>
        /// <returns>Ordered identifiers of this variants.</returns>
        IEnumerable<TVariantID> GetOrderedIDs(IEnumerable<TSnapshot> variants);

        /// <summary>
        /// Gets display name of this variant.
        /// </summary>
        /// <param name="variant">The variant of current enum.</param>
        /// <returns>The display name of this variant.</returns>
        string GetVariantDisplayName(TSnapshot variant);

        /// <summary>
        /// Sends request for getting variants of the current enum.
        /// </summary>
        /// <param name="projectSnapshot">Projects Service for Forge requests.</param>
        /// <returns>All variants of the current enum for this user.</returns>
        IAsyncEnumerable<TSnapshot> GetVariantsFromRemote(ProjectSnapshot projectSnapshot);

        /// <summary>
        /// Gets snapshots of variants from a given project snapshot.
        /// </summary>
        /// <param name="project">The source snapshot of project.</param>
        /// <returns>The enumeration of the snapshots of this enum.</returns>
        IEnumerable<TSnapshot> GetSnapshots(ProjectSnapshot project);

        /// <summary>
        /// Deserializes a stored identifier with multiple variant IDs.
        /// </summary>
        /// <param name="externalID">The stored identifier with multiple variant IDs.</param>
        /// <returns>Multiple variant IDs.</returns>
        IEnumerable<TVariantID> DeserializeID(string externalID);
    }
}
