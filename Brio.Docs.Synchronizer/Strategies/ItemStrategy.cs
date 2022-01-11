using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Synchronization.Strategies
{
    internal static class ItemStrategy
    {
        public static void UpdateExternalIDs(IEnumerable<Item> local, ICollection<Item> remote)
        {
            foreach (var item in local.Where(x => string.IsNullOrWhiteSpace(x.ExternalID)))
                item.ExternalID = remote.FirstOrDefault(x => x.RelativePath == item.RelativePath)?.ExternalID;
        }
    }
}
