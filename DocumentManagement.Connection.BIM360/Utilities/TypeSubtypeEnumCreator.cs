using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class TypeSubtypeEnumCreator
        : IEnumCreator<IssueSubtype, IssueTypeSnapshot, (string parentTypeID, string subtypeID)>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            $"{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.NgIssueTypeID)},{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.NgIssueSubtypeID)}";

        private static readonly string DISPLAY_NAME = "Type";

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => false;

        public string NullID => throw new NotSupportedException();

        public IOrderedEnumerable<(string parentTypeID, string subtypeID)> GetOrderedIDs(
            IEnumerable<IssueTypeSnapshot> variants)
            => variants
               .Select(x => (type: x.ParentType.ID, subtype: x.Subtype.ID))
               .OrderBy(id => id.type)
               .ThenBy(id => id.subtype);

        public string GetVariantDisplayName((IssueType parentType, IssueSubtype subtype) type)
            => string.Equals(type.parentType.Title, type.subtype.Title, StringComparison.Ordinal)
                ? type.parentType.Title
                : $"{type.parentType.Title}: {type.subtype.Title}";

        public string GetVariantDisplayName(IssueTypeSnapshot snapshot)
            => string.Equals(snapshot.ParentType.Title, snapshot.Subtype.Title, StringComparison.Ordinal)
                ? snapshot.ParentType.Title
                : $"{snapshot.ParentType.Title}: {snapshot.Subtype.Title}";

        public async Task<IEnumerable<IssueTypeSnapshot>> GetVariantsFromRemote(
            IssuesService issuesService,
            ProjectSnapshot projectSnapshot)
            => (await issuesService.GetIssueTypesAsync(projectSnapshot.IssueContainer)).SelectMany(
                x => x.Subtypes.Select(y => new IssueTypeSnapshot(x, y, projectSnapshot)));

        public IEnumerable<IssueTypeSnapshot> GetSnapshots(IEnumerable<ProjectSnapshot> projects)
            => projects.SelectMany(x => x.IssueTypes.Values);

        public IEnumerable<(string parentTypeID, string subtypeID)> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<(string parentTypeID, string subtypeID)>(externalID);
    }
}
