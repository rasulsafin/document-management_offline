using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class LocationService
    {
        private readonly ForgeConnection connection;

        public LocationService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<List<Location>> GetLocationsAsync(string containerID, string treeID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetLocationMethod,
                containerID,
                treeID);

            var list = response[RESULTS_PROPERTY]?.ToObject<List<Location>>();
            list.RemoveAt(0);

            return list;
        }
    }
}
