using System;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models
{
    internal class IssueTypeSnapshot : AEnumVariantSnapshot<IssueSubtype>
    {
        public IssueTypeSnapshot(IssueType parentType, IssueSubtype subtype, ProjectSnapshot projectSnapshot)
            : base(subtype, projectSnapshot)
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
