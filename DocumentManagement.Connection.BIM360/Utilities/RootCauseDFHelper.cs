using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class RootCauseDFHelper : IDFHelper<RootCause, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.RootCause);

        private static readonly string DISPLAY_NAME = "Root Cause";

        public string ID => ENUM_EXTERNAL_ID;

        public string DisplayName => DISPLAY_NAME;

        public IOrderedEnumerable<string> Order(IEnumerable<RootCause> types)
            => types.Select(cause => cause.Attributes.Key).OrderBy(id => id);

        public string GetDisplayName(RootCause type)
            => type.Attributes.Title;

        public async Task<IEnumerable<RootCause>> GetFromRemote(IssuesService issuesService, ProjectSnapshot projectSnapshot)
            => await issuesService.GetRootCausesAsync(projectSnapshot.IssueContainer);
    }
}
