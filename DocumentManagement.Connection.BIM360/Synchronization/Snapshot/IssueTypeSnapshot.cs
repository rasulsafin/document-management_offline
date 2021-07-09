using System;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    internal class IssueTypeSnapshot : ASnapshotEntity<IssueSubtype>
    {
        private string id;

        public IssueTypeSnapshot(IssueType parentType, IssueSubtype subtype)
            : base(subtype)
        {
            ParentType = parentType;
            SubTypeIsType = string.Equals(parentType.Title, subtype.Title, StringComparison.Ordinal);
        }

        public IssueType ParentType { get; }

        public string ParentTypeID => ParentType.ID;

        public string SubtypeID => Entity.ID;

        public override string ID
        {
            get => id;
        }

        public bool SubTypeIsType { get; }

        public void SetExternalID(string externalID)
            => id = externalID;
    }
}
