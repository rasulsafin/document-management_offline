using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models.DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Connection.BIM360.Forge.Services
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
    }
}
