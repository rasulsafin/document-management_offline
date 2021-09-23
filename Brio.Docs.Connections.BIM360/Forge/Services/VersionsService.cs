using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Properties;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class VersionsService
    {
        private readonly ForgeConnection connection;

        public VersionsService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<(Item item, Version version)> PostVersionAsync(string projectId, Version version)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPostWithJsonApi(version),
                    Resources.PostProjectsVersionMethod,
                    projectId);
            var resultVersion = response[DATA_PROPERTY]?.ToObject<Version>();
            var includedItems = response[INCLUDED_PROPERTY]?.ToObject<List<Item>>();
            var item = includedItems?.FirstOrDefault();

            return (item, resultVersion);
        }
    }
}
