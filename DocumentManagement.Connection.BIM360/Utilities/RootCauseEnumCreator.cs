using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class RootCauseEnumCreator : IEnumCreator<RootCause, RootCauseSnapshot, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.RootCause);

        private static readonly string DISPLAY_NAME = MrsConstants.ROOT_CAUSE_FIELD_NAME;

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => true;

        public string NullID => $"{EnumExternalID}{DynamicFieldUtilities.NULL_VALUE_ID}";

        public IOrderedEnumerable<string> GetOrderedIDs(IEnumerable<RootCauseSnapshot> variants)
            => variants.Select(cause => cause.Entity.ID).OrderBy(id => id);

        public string GetVariantDisplayName(RootCauseSnapshot variant)
            => variant.Entity.Attributes.Title;

        public async Task<IEnumerable<RootCauseSnapshot>> GetVariantsFromRemote(
            IssuesService issuesService,
            ProjectSnapshot projectSnapshot)
            => (await issuesService.GetRootCausesAsync(projectSnapshot.IssueContainer)).Select(
                x => new RootCauseSnapshot(x, projectSnapshot));

        public IEnumerable<RootCauseSnapshot> GetSnapshots(IEnumerable<ProjectSnapshot> projects)
            => projects.SelectMany(x => x.RootCauses.Values);

        public IEnumerable<string> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
