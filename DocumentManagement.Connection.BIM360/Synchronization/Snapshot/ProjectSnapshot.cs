using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    internal class ProjectSnapshot : ASnapshotEntity<Project>
    {
        public ProjectSnapshot(Project entity)
            : base(entity)
        {
        }

        public string IssueContainer => Entity.Relationships.IssuesContainer.Data.ID;

        public Dictionary<string, IssueSnapshot> Issues { get; set; }

        public Dictionary<string, IssueType> IssueTypes { get; set; }

        public Dictionary<string, ItemSnapshot> Items { get; set; }

        public Folder ProjectFilesFolder { get; set; }

        public override string ID => Entity.ID;
    }
}
