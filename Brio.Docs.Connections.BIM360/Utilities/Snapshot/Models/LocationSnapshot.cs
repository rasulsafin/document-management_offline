using Brio.Docs.Connections.Bim360.Forge.Models;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal class LocationSnapshot : AEnumVariantSnapshot<Location>
    {
        public LocationSnapshot(Location entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
