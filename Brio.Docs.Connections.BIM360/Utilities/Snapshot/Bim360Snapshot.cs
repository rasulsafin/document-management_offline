using System.Collections.Generic;
using System.Linq;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class Bim360Snapshot
    {
        public Dictionary<string, HubSnapshot> Hubs { get; set; }

        public IEnumerable<ProjectSnapshot> ProjectEnumerable => Hubs.SelectMany(x => x.Value.Projects.Values);

        public IEnumerable<IssueSnapshot> IssueEnumerable => ProjectEnumerable.SelectMany(x => x.Issues.Values);
    }
}
