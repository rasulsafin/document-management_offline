using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class StatusSnapshot : AEnumVariantSnapshot<Status>
    {
        public StatusSnapshot(Status entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
