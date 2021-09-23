using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class ProjectSnapshot : ASnapshotEntity<Project>
    {
        public ProjectSnapshot(Project entity, HubSnapshot hubSnapshot)
            : base(entity)
            => HubSnapshot = hubSnapshot;

        public HubSnapshot HubSnapshot { get; }

        public string IssueContainer => Entity.Relationships.IssuesContainer.Data.ID;

        public Dictionary<string, IssueSnapshot> Issues { get; set; }

        public Dictionary<string, IssueTypeSnapshot> IssueTypes { get; set; }

        public Dictionary<string, RootCauseSnapshot> RootCauses { get; set; }

        public Dictionary<string, AssignToVariant> AssignToVariants { get; set; }

        public Dictionary<string, ItemSnapshot> Items { get; set; }

        public string MrsFolderID { get; set; }

        public override string ID => Entity.ID;
    }
}
