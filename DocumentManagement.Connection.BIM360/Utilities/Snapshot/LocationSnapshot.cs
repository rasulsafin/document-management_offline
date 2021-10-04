using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class LocationSnapshot : AEnumVariantSnapshot<Location>
    {
        public LocationSnapshot(Location entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
