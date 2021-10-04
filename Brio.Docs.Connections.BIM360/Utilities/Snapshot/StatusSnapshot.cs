using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class StatusSnapshot : AEnumVariantSnapshot<Status>
    {
        public StatusSnapshot(Status entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }
    }
}
