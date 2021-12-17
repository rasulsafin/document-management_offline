using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Utils.Pagination;
using Brio.Docs.Connections.Bim360.Properties;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class LocationService
    {
        private readonly ForgeConnection connection;

        public LocationService(ForgeConnection connection)
            => this.connection = connection;

        public async IAsyncEnumerable<Location> GetLocationsAsync(string containerID, string treeID, IEnumerable<Filter> filters = null)
        {
            var locations = PaginationHelper.GetItemsByPages<Location, PaginationStrategy>(
                connection,
                ForgeConnection.SetParameters(Resources.GetLocationMethod, filters),
                RESULTS_PROPERTY,
                containerID,
                treeID);

            if (locations == null)
                yield break;

            await foreach (var location in locations)
            {
                if (location.Type != LOCATION_TREE_ROOT)
                    yield return location;
            }
        }
    }
}
