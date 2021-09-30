using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
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
