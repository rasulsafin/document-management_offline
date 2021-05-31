using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    public class IssueSnapshot : ASnapshotEntity<Issue>
    {
        public IssueSnapshot(Issue entity)
            : base(entity)
        {
        }

        public List<Item> Type { get; set; }
    }
}
