using System.Net.Http;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge;
using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models;
using Forge.Models.DataManagement;
using static Forge.Constants;

namespace Forge.Services
{
    public class ItemsService
    {
        private readonly Connection connection;

        public ItemsService(Connection connection)
            => this.connection = connection;

        public async Task<(Item, Version)> PostItemAsync(string projectId, Item item, Version version)
        {
            var response = await connection.SendRequestWithSerializedData(HttpMethod.Post,
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
                .SendRequestWithSerializedData(HttpMethod.Post, Resources.PostProjectsVersionMethod, version, projectId);
            var resultVersion = response[DATA_PROPERTY]?.ToObject<Version>();
            var item = response[INCLUDED_PROPERTY]?.ToObject<Item>();

            return (item, resultVersion);
        }
    }
}
