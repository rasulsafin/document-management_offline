using Brio.Docs.Connection.Bim360.Forge.Models.DataManagement;

namespace Brio.Docs.Connection.Bim360.Utilities.Snapshot
{
    internal class ItemSnapshot : ASnapshotEntity<Item>
    {
        public ItemSnapshot(Item entity)
            : base(entity)
        {
        }

        public Version Version { get; set; }

        public override string ID => Entity.ID;
    }
}
