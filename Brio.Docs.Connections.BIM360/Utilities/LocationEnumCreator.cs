using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Utilities
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

        public IAsyncEnumerable<LocationSnapshot> GetVariantsFromRemote(
            ProjectSnapshot projectSnapshot)
            => locationService.GetLocationsAsync(projectSnapshot.LocationContainer, DEFAULT_LOCATION_TREE_ID)
               .Select(x => new LocationSnapshot(x, projectSnapshot));

        public IEnumerable<LocationSnapshot> GetSnapshots(ProjectSnapshot project)
            => project.Locations.Values;

        public IEnumerable<string> DeserializeID(string externalID)
             => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
