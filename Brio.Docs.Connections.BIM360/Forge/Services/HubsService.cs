using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class HubsService
    {
        private readonly ForgeConnection connection;

        public HubsService(ForgeConnection connection)
        {
            this.connection = connection;
        }

        public async Task<List<Hub>> GetHubsAsync()
        {
            var response = await connection.SendAsync(ForgeSettings.AuthorizedGet(), Resources.GetHubsMethod);
            var data = response[DATA_PROPERTY]?.ToObject<List<Hub>>();

            return data ?? new List<Hub>();
        }

        public async Task<Hub> GetHubInfoAsync(string hubId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetHubsInfoByIdMethod,
                    hubId);
            return response[DATA_PROPERTY]?.ToObject<Hub>();
        }
    }
}
