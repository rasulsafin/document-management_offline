using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    internal class Bim360Snapshot
    {
        public Dictionary<string, HubSnapshot> Hubs { get; set; }

        public IEnumerable<KeyValuePair<string, ProjectSnapshot>> ProjectEnumerable
            => Hubs.SelectMany(x => x.Value.Projects);
    }
}
