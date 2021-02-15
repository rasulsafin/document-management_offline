using System.Net.Http;
using System.Threading.Tasks;
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
            var response = await connection.SendSerializedDataAndIncludedAuthorizedAsync(HttpMethod.Post,
                    Resources.PostProjectsItemsMethod,
                    item,
                    new[] { version },
                    projectId);

            return (response[DATA_PROPERTY]?.ToObject<Item>(), response[INCLUDED_PROPERTY]?.First?.ToObject<Version>());
        }
    }
}
