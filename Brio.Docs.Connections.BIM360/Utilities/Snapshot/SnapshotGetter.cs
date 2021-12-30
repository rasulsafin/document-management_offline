using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class SnapshotGetter
    {
        private readonly Bim360Snapshot bim360Snapshot;

        public SnapshotGetter(Bim360Snapshot bim360Snapshot)
        {
            this.bim360Snapshot = bim360Snapshot;
        }

        public ProjectSnapshot GetProject(string id)
            => bim360Snapshot.ProjectEnumerable.FirstOrDefault(x => x.ID == id);

        public IEnumerable<IssueSnapshot> GetIssues()
            => bim360Snapshot.IssueEnumerable;

        public IssueSnapshot GetIssue(string id)
            => bim360Snapshot.IssueEnumerable.FirstOrDefault(x => x.ID == id);

        public IssueSnapshot GetIssue(ProjectSnapshot project, string id)
            => project.Issues.TryGetValue(id, out var result) ? result : null;

        public IEnumerable<ProjectSnapshot> GetProjects()
            => bim360Snapshot.ProjectEnumerable;

        public ItemSnapshot GetItem(ProjectSnapshot project, string id)
            => project.Items.TryGetValue(id, out var item) ? item : null;
    }
}
