using System;
using System.IO;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
{
    internal static class ItemExtensions
    {
        /// <summary>
        /// Gets the captured item with the same name.
        /// </summary>
        /// <param name="projectSnapshot">The captured project.</param>
        /// <param name="name">The name of the item to find. </param>
        /// <param name="ignoreExtensions">True if the search should ignore the extensions of the captured items.
        /// The search name extension will not be ignored.</param>
        /// <returns>Captured item.</returns>
        public static ItemSnapshot FindItemByName(
            this ProjectSnapshot projectSnapshot,
            string name,
            bool ignoreExtensions = false)
        {
            string GetFileName(string fileName)
                => ignoreExtensions ? Path.GetFileNameWithoutExtension(fileName) : fileName;

            return projectSnapshot.Items.FirstOrDefault(
                    x => string.Equals(
                            GetFileName(x.Value.Entity.Attributes.DisplayName),
                            name,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(
                            GetFileName(x.Value.Version.Attributes.Name),
                            name,
                            StringComparison.InvariantCultureIgnoreCase))
               .Value;
        }
    }
}
