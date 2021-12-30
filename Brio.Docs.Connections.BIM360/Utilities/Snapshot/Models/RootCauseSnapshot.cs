using Brio.Docs.Connections.Bim360.Forge.Models;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal class RootCauseSnapshot : AEnumVariantSnapshot<RootCause>
    {
        public RootCauseSnapshot(RootCause entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
