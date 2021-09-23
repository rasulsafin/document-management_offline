using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class TypeSubtypeEnumCreator
        : IEnumCreator<IssueSubtype, IssueTypeSnapshot, (string parentTypeID, string subtypeID)>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            $"{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.NgIssueTypeID)},{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.NgIssueSubtypeID)}";

        private static readonly string DISPLAY_NAME = MrsConstants.TYPE_FIELD_NAME;

        private readonly IssuesService issuesService;

        public TypeSubtypeEnumCreator(IssuesService issuesService)
            => this.issuesService = issuesService;

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => false;

        public string NullID => throw new NotSupportedException();

        public IEnumerable<(string parentTypeID, string subtypeID)> GetOrderedIDs(
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

        public async Task<IEnumerable<IssueTypeSnapshot>> GetVariantsFromRemote(ProjectSnapshot projectSnapshot)
            => (await issuesService.GetIssueTypesAsync(projectSnapshot.IssueContainer)).SelectMany(
                x => x.Subtypes.Select(y => new IssueTypeSnapshot(x, y, projectSnapshot)));

        public IEnumerable<IssueTypeSnapshot> GetSnapshots(ProjectSnapshot project)
            => project.IssueTypes.Values;

        public IEnumerable<(string parentTypeID, string subtypeID)> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<(string parentTypeID, string subtypeID)>(externalID);
    }
}
