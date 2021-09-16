using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class IssueSnapshot : ASnapshotEntity<Issue>
    {
        public IssueSnapshot(Issue entity, ProjectSnapshot projectSnapshot)
            : base(entity)
        {
            ProjectSnapshot = projectSnapshot;
        }

        public Dictionary<string, ItemSnapshot> Items { get; set; }

        public ProjectSnapshot ProjectSnapshot { get; }

        public override string ID => Entity.ID;
    }
}
