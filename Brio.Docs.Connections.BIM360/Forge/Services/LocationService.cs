using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<List<Location>> GetLocationsAsync(string containerID, string treeID, IEnumerable<Filter> filters = null)
        {
            var locations = await PaginationHelper.GetItemsByPages<Location, PaginationStrategy>(
                connection,
                ForgeConnection.SetParameters(Resources.GetLocationMethod, filters),
                RESULTS_PROPERTY,
                containerID,
                treeID);

            if (locations != null)
            {
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locations[i].Type == LOCATION_TREE_ROOT)
                    {
                        locations.RemoveAt(i);
                        break;
                    }
                }
            }

            return locations;
        }
    }
}
