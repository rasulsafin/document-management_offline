using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal class HubSnapshot : ASnapshotEntity<Hub>
    {
        public HubSnapshot(Hub entity)
            : base(entity)
        {
        }

        public Dictionary<string, ProjectSnapshot> Projects { get; set; }

        public Dictionary<string, User> Users { get; set; }

        public override string ID => Entity.ID;
    }
}
