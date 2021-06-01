using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    public class HubSnapshot : ASnapshotEntity<Hub>
    {
        public HubSnapshot(Hub entity)
            : base(entity)
        {
        }

        public Dictionary<string, ProjectSnapshot> Projects { get; set; }

        public override string ID => Entity.ID;
    }
}
