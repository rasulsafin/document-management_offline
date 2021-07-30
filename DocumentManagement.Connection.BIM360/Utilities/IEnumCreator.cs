using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    /// <summary>
    /// Represents a class for working with BIM 360 properties such as dynamic field.
    /// </summary>
    /// <typeparam name="TVariant">The type of BIM 360 property.</typeparam>
    /// <typeparam name="TVariantID">The identifier to find needed variant</typeparam>
    internal interface IEnumCreator<TVariant, out TVariantID>
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
        /// Gets ordered identifiers of this variants.
        /// </summary>
        /// <param name="variants">Variants required to get identifiers.</param>
        /// <returns>Ordered identifiers of this variants.</returns>
        IOrderedEnumerable<TVariantID> GetOrderedIDs(IEnumerable<TVariant> variants);

        /// <summary>
        /// Gets display name of this variant.
        /// </summary>
        /// <param name="variant">The variant of current enum.</param>
        /// <returns>The display name of this variant.</returns>
        string GetVariantDisplayName(TVariant variant);

        /// <summary>
        /// Sends request for getting variants of the current enum.
        /// </summary>
        /// <param name="issuesService">Issues Service for Forge requests.</param>
        /// <param name="projectSnapshot">Projects Service for Forge requests.</param>
        /// <returns>All variants of the current enum for this user.</returns>
        Task<IEnumerable<TVariant>> GetVariantsFromRemote(IssuesService issuesService, ProjectSnapshot projectSnapshot);
    }
}
