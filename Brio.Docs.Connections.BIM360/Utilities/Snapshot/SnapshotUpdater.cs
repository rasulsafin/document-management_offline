using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class SnapshotUpdater
    {
        private readonly SnapshotGetter snapshot;

        public SnapshotUpdater(SnapshotGetter snapshot)
        {
            this.snapshot = snapshot;
        }

        public IssueSnapshot UpdateIssue(Issue issue)
        {
            var issueSnapshot = snapshot.GetIssue(issue.ID);
            issueSnapshot.Entity = issue;
            RemoveFromProjectIfNeeded(issueSnapshot);
            return issueSnapshot;
        }

        public IssueSnapshot UpdateIssue(IssueSnapshot issueSnapshot, Issue issue)
        {
            issueSnapshot.Entity = issue;
            RemoveFromProjectIfNeeded(issueSnapshot);
            return issueSnapshot;
        }

        public IssueSnapshot CreateIssue(ProjectSnapshot project, Issue issue)
        {
            var issueSnapshot = new IssueSnapshot(issue, project);
            if (!IssueUtilities.IsRemoved(issueSnapshot.Entity))
                project.Issues.Add(issue.ID, issueSnapshot);
            return issueSnapshot;
        }

        public ItemSnapshot CreateItem(ProjectSnapshot project, Item item, Version version)
        {
            var itemSnapshot = new ItemSnapshot(item, version);
            project.Items.Add(item.ID, itemSnapshot);
            return itemSnapshot;
        }

        public ItemSnapshot UpdateItem(ProjectSnapshot projectSnapshot, ItemSnapshot itemSnapshot, Item item, Version version)
        {
            if (itemSnapshot.ID != item.ID)
            {
                projectSnapshot.Items.Remove(itemSnapshot.ID);
                itemSnapshot = new ItemSnapshot(item, version);
                projectSnapshot.Items.Add(item.ID, itemSnapshot);
            }
            else
            {
                itemSnapshot.Entity = item;
                itemSnapshot.Version = version;
            }

            return itemSnapshot;
        }

        private void RemoveFromProjectIfNeeded(IssueSnapshot issueSnapshot)
        {
            var dictionary = issueSnapshot.ProjectSnapshot.Issues;
            if (IssueUtilities.IsRemoved(issueSnapshot.Entity) &&
                dictionary.ContainsKey(issueSnapshot.ID))
                dictionary.Remove(issueSnapshot.ID);
        }
    }
}
