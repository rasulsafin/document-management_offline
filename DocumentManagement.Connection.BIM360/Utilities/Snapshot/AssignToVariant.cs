using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class AssignToVariant : AEnumVariantSnapshot<string>
    {
        public AssignToVariant(string id, AssignToType type, string title, ProjectSnapshot projectSnapshot)
            : base(id, projectSnapshot)
        {
            Type = type;
            Title = title;
        }

        public string Title { get; }

        public AssignToType Type { get; }
    }
}
