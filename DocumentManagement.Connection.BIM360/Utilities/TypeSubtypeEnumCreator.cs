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
    internal class TypeSubtypeEnumCreator : IEnumCreator<(IssueType parentType, IssueSubtype subtype), (string parentTypeID, string subtypeID)>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            $"{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.NgIssueTypeID)},{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.NgIssueSubtypeID)}";

        private static readonly string DISPLAY_NAME = "Type";

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public IOrderedEnumerable<(string parentTypeID, string subtypeID)> GetOrderedIDs(
            IEnumerable<(IssueType parentType, IssueSubtype subtype)> types)
            => types.Select(x => (type: x.parentType.ID, subtype: x.subtype.ID))
               .OrderBy(id => id.type)
               .ThenBy(id => id.subtype);

        public string GetVariantDisplayName((IssueType parentType, IssueSubtype subtype) type)
            => string.Equals(type.parentType.Title, type.subtype.Title, StringComparison.Ordinal)
                ? type.parentType.Title
                : $"{type.parentType.Title}: {type.subtype.Title}";

        public async Task<IEnumerable<(IssueType parentType, IssueSubtype subtype)>> GetVariantsFromRemote(IssuesService issuesService, ProjectSnapshot projectSnapshot)
            => (await issuesService.GetIssueTypesAsync(projectSnapshot.IssueContainer)).SelectMany(
                x => x.Subtypes.Select(y => (x, y)));
    }
}
