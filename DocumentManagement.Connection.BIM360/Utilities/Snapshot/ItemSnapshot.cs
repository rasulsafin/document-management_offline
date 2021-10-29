using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
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
    }
}
