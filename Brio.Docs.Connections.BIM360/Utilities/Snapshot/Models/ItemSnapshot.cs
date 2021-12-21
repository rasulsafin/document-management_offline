using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Synchronization.Models;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal class ItemSnapshot : ASnapshotEntity<Item>
    {
        public ItemSnapshot(Item entity, Version version)
            : base(entity)
        {
            Version = version;
        }

        public Version Version { get; set; }

        public override string ID => Entity.ID;

        public IfcConfig Config { get; set; }

        public Vector3d? GlobalOffset { get; set; }
    }
}
