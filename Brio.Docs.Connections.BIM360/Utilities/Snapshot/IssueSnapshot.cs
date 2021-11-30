using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Integration.Dtos;

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

        public IEnumerable<BimElementExternalDto> BimElements { get; set; }

        public List<CommentSnapshot> Comments { get; set; }

        public ProjectSnapshot ProjectSnapshot { get; }

        public override string ID => Entity.ID;
    }
}
