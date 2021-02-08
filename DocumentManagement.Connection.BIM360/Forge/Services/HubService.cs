using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge;
using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models.DataManagement;
using static Forge.Constants;

namespace Forge.Services
{
    public class HubService
    {
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

        public async Task<Hub> GetHubInfoAsync(string hubId)
        {
            var response = await connection.GetResponse(HttpMethod.Get, Resources.GetHubsInfoByIdMethod, hubId);
            var data = response[DATA_PROPERTY]?.ToObject<Hub>();

            return data;
        }
    }
}
