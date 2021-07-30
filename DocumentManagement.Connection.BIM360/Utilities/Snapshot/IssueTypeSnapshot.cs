using System;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class IssueTypeSnapshot : AEnumVariantSnapshot<IssueSubtype>
    {
        public IssueTypeSnapshot(IssueType parentType, IssueSubtype subtype)
            : base(subtype)
        {
            ParentType = parentType;
            SubTypeIsType = string.Equals(parentType.Title, subtype.Title, StringComparison.Ordinal);
        }

        public IssueType ParentType { get; }

        public IssueSubtype Subtype => Entity;

        public string ParentTypeID => ParentType.ID;

        public string SubtypeID => Entity.ID;

        public bool SubTypeIsType { get; }
    }
}
