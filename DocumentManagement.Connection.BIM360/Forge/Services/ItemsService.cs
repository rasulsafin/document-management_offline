using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class ItemsService
    {
        private readonly ForgeConnection connection;

        public ItemsService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<(Item item, Version version)> PostItemAsync(string projectId, Item item, Version version)
        {
            var response = await connection.SendSerializedDataAuthorizedAsync(HttpMethod.Post,
                    Resources.PostProjectsItemsMethod,
                    new
                    {
                        jsonapi = JsonApi.Default,
                        data = item,
                        included = new[] { version },
                    },
                    projectId);

            return (response[DATA_PROPERTY]?.ToObject<Item>(), response[INCLUDED_PROPERTY]?.First?.ToObject<Version>());
        }

        public async Task<(Item, Version)> PostVersionAsync(string projectId, Version version)
        {
            var response = await connection
                .SendSerializedDataAuthorizedAsync(HttpMethod.Post, Resources.PostProjectsVersionMethod, version, projectId);
            var resultVersion = response[DATA_PROPERTY]?.ToObject<Version>();
            var item = response[INCLUDED_PROPERTY]?.ToObject<Item>();

            return (item, resultVersion);
        }
    }
}
