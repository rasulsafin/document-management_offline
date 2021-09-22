using Brio.Docs.Connection.Bim360.Forge.Models;

namespace Brio.Docs.Connection.Bim360.Utilities.Snapshot
{
    internal class RootCauseSnapshot : AEnumVariantSnapshot<RootCause>
    {
        public RootCauseSnapshot(RootCause entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
