using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class RootCauseEnumCreator : IEnumCreator<RootCause, RootCause, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.RootCause);

        private static readonly string DISPLAY_NAME = "Root Cause";

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public IOrderedEnumerable<string> GetOrderedIDs(IEnumerable<RootCause> types)
            => types.Select(cause => cause.Attributes.Key).OrderBy(id => id);

        public string GetVariantDisplayName(RootCause type)
            => type.Attributes.Title;

        public string GetVariantDisplayName(AEnumVariantSnapshot<RootCause> snapshot)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<RootCause>> GetVariantsFromRemote(IssuesService issuesService, ProjectSnapshot projectSnapshot)
            => await issuesService.GetRootCausesAsync(projectSnapshot.IssueContainer);

        public RootCause GetMain(RootCause variant)
            => variant;

        public RootCause GetVariant(AEnumVariantSnapshot<RootCause> snapshot)
            => snapshot.Entity;
    }
}
