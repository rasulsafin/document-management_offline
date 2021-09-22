using Brio.Docs.Connection.Bim360.Forge.Models.DataManagement;
using System.Collections.Generic;

namespace Brio.Docs.Connection.Bim360.Utilities.Snapshot
{
    internal class HubSnapshot : ASnapshotEntity<Hub>
    {
        public HubSnapshot(Hub entity)
            : base(entity)
        {
        }

        public Dictionary<string, ProjectSnapshot> Projects { get; set; }

        public override string ID => Entity.ID;
    }
}
