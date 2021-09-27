using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal class LocationEnumCreator : IEnumCreator<Location, LocationSnapshot, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.LbsLocation);

        private static readonly string DISPLAY_NAME = MrsConstants.LOCATION;

        private readonly LocationService locationService;

        public LocationEnumCreator(LocationService locationService)
            => this.locationService = locationService;

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => true;

        public string NullID => $"{EnumExternalID}{DynamicFieldUtilities.NULL_VALUE_ID}";

        public IEnumerable<string> GetOrderedIDs(IEnumerable<LocationSnapshot> variants)
            => variants.Select(cause => cause.Entity.ID).OrderBy(id => id);

        public string GetVariantDisplayName(LocationSnapshot variant)
            => variant.Entity.Name;

        public async Task<IEnumerable<LocationSnapshot>> GetVariantsFromRemote(
            ProjectSnapshot projectSnapshot)
            => (await locationService.GetLocationsAsync(projectSnapshot.IssueContainer, DEFAULT_LOCATION_TREE_ID)).Select(
                    x => new LocationSnapshot(x, projectSnapshot));

        public IEnumerable<LocationSnapshot> GetSnapshots(ProjectSnapshot project)
            => project.Locations.Values;

        public IEnumerable<string> DeserializeID(string externalID)
             => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
