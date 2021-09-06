using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class AssignToVariant : AEnumVariantSnapshot<ObjectInfo>
    {
        public AssignToVariant(ObjectInfo entity, ProjectSnapshot projectSnapshot)
            : base(entity, projectSnapshot)
        {
        }

        public string Title { get; set; }
    }
}
