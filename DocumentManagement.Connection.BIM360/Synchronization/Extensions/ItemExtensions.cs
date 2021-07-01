using System;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
{
    internal static class ItemExtensions
    {
        public static ItemSnapshot FindItemByName(this ProjectSnapshot projectSnapshot, string name)
        {
            return projectSnapshot.Items.FirstOrDefault(
                    x => string.Equals(
                            x.Value.Entity.Attributes.DisplayName,
                            name,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(
                            x.Value.Version.Attributes.Name,
                            name,
                            StringComparison.InvariantCultureIgnoreCase))
               .Value;
        }
    }
}
