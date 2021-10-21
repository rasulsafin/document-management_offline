using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class RootCauseEnumCreator : IEnumCreator<RootCause, RootCauseSnapshot, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.RootCause);

        private static readonly string DISPLAY_NAME = MrsConstants.ROOT_CAUSE_FIELD_NAME;

        private readonly IssuesService issuesService;

        public RootCauseEnumCreator(IssuesService issuesService)
            => this.issuesService = issuesService;

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => true;

        public string NullID => $"{EnumExternalID}{DynamicFieldUtilities.NULL_VALUE_ID}";

        public IEnumerable<string> GetOrderedIDs(IEnumerable<RootCauseSnapshot> variants)
            => variants.Select(cause => cause.Entity.ID).OrderBy(id => id);

        public string GetVariantDisplayName(RootCauseSnapshot variant)
            => variant.Entity.Attributes.Title;

        public async Task<IEnumerable<RootCauseSnapshot>> GetVariantsFromRemote(
            ProjectSnapshot projectSnapshot)
            => (await issuesService.GetRootCausesAsync(projectSnapshot.IssueContainer)).Select(
                x => new RootCauseSnapshot(x, projectSnapshot));

        public IEnumerable<RootCauseSnapshot> GetSnapshots(ProjectSnapshot project)
            => project.RootCauses.Values;

        public IEnumerable<string> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
