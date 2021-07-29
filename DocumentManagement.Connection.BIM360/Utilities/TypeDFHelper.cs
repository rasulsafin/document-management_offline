using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class TypeDFHelper : IDFHelper<(IssueType parentType, IssueSubtype subtype), (string parentTypeID, string subtypeID)>
    {
        private static readonly string ENUM_EXTERNAL_ID = "ng_issue_type_id,ng_issue_subtype_id";
        private static readonly string DISPLAY_NAME = "Type";

        public string ID => ENUM_EXTERNAL_ID;

        public string DisplayName => DISPLAY_NAME;

        public IOrderedEnumerable<(string parentTypeID, string subtypeID)> Order(
            IEnumerable<(IssueType parentType, IssueSubtype subtype)> types)
            => types.Select(x => (type: x.parentType.ID, subtype: x.subtype.ID))
               .OrderBy(id => id.type)
               .ThenBy(id => id.subtype);

        public string GetDisplayName((IssueType parentType, IssueSubtype subtype) type)
            => string.Equals(type.parentType.Title, type.subtype.Title, StringComparison.Ordinal)
                ? type.parentType.Title
                : $"{type.parentType.Title}: {type.subtype.Title}";

        public async Task<IEnumerable<(IssueType parentType, IssueSubtype subtype)>> GetFromRemote(IssuesService issuesService, ProjectSnapshot projectSnapshot)
            => (await issuesService.GetIssueTypesAsync(projectSnapshot.IssueContainer)).SelectMany(
                x => x.Subtypes.Select(y => (x, y)));
    }
}
