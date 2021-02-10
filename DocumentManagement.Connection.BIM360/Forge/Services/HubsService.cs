using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge.Models.DataManagement;
using DocumentManagement.Connection.BIM360.Properties;
using static DocumentManagement.Connection.BIM360.Forge.Constants;

namespace DocumentManagement.Connection.BIM360.Forge.Services
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
            var response = await connection.GetResponseAuthorizedAsync(HttpMethod.Get, Resources.GetHubsMethod);
            var data = response[DATA_PROPERTY]?.ToObject<List<Hub>>();

            return data ?? new List<Hub>();
        }

        public async Task<Hub> GetHubInfoAsync(string hubId)
        {
            var response = await connection.GetResponseAuthorizedAsync(HttpMethod.Get, Resources.GetHubsInfoByIdMethod, hubId);
            var data = response[DATA_PROPERTY]?.ToObject<Hub>();

            return data;
        }
    }
}
