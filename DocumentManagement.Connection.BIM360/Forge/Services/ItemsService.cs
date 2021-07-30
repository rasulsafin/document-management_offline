using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class ItemsService
    {
        private readonly ForgeConnection connection;

        public ItemsService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<(Item item, Version version)> GetAsync(string projectId, string itemID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetProjectsItemMethod,
                projectId,
                itemID);

            var versions = response[INCLUDED_PROPERTY]?.ToObject<IEnumerable<Version>>();
            return (response[DATA_PROPERTY]?.ToObject<Item>(), versions.Max());
        }

        public async Task<(Item item, Version version)> PostItemAsync(string projectId, Item item, Version version)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPostWithJsonApi(item, new[] { version }),
                    Resources.PostProjectsItemsMethod,
                    projectId);

            return (response[DATA_PROPERTY]?.ToObject<Item>(), response[INCLUDED_PROPERTY]?.First?.ToObject<Version>());
        }

        public async Task<List<Version>> GetVersions(string projectID, string itemID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetProjectsItemsVersionsMethod,
                projectID,
                itemID);

            return response[DATA_PROPERTY]?.ToObject<List<Version>>();
        }
    }
}
