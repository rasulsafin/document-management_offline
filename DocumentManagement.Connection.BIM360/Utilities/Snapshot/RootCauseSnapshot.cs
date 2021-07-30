using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class RootCauseSnapshot : AEnumVariantSnapshot<RootCause>
    {
        public RootCauseSnapshot(RootCause entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
