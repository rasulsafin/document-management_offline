using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class IssueSnapshot : ASnapshotEntity<Issue>
    {
        public IssueSnapshot(Issue entity, ProjectSnapshot projectSnapshot)
            : base(entity)
        {
            ProjectSnapshot = projectSnapshot;
        }

        public Dictionary<string, Attachment> Attachments { get; set; }

        public ProjectSnapshot ProjectSnapshot { get; }

        public override string ID => Entity.ID;
    }
}
