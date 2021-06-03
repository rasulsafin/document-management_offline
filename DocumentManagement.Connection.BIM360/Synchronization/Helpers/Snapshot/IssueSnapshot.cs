using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    public class IssueSnapshot : ASnapshotEntity<Issue>
    {
        public IssueSnapshot(Issue entity, ProjectSnapshot projectSnapshot)
            : base(entity)
        {
            ProjectSnapshot = projectSnapshot;
        }

        public List<ItemSnapshot> Items { get; set; }

        public ProjectSnapshot ProjectSnapshot { get; }

        public override string ID => Entity.ID;
    }
}
