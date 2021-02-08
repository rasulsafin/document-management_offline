using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge;
using DocumentManagement.Connection.BIM360.Properties;

namespace Forge.Services
{
    public class HubService
    {
        private static readonly string DATA_PROPERTY = "data";
        private readonly Connection connection;

        public HubService(Connection connection)
        {
            this.connection = connection;
        }

        public async Task<List<Hub>> GetHubsAsync()
        {
            var response = await connection.GetResponse(HttpMethod.Get, Resources.GetHubsMethod);
            var data = response[DATA_PROPERTY]?.ToObject<List<Hub>>();

            return data ?? new List<Hub>();
        }

        public async Task<Hub> GetHubInfo(string hubId)
        {
            var response = await connection.GetResponse(HttpMethod.Get, Resources.GetHubsInfoByIdMethod, hubId);
            var data = response[DATA_PROPERTY]?.ToObject<Hub>();

            return data;
        }
    }
}
