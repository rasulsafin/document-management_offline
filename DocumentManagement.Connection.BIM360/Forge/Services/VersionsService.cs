using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class VersionsService
    {
        private readonly ForgeConnection connection;

        public VersionsService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<(Item item, Version version)> PostVersionAsync(string projectId, Version version)
        {
            var response = await connection
                .SendSerializedDataAuthorizedAsync(HttpMethod.Post, Resources.PostProjectsVersionMethod, version, projectId);
            var resultVersion = response[DATA_PROPERTY]?.ToObject<Version>();
            var includedItems = response[INCLUDED_PROPERTY]?.ToObject<List<Item>>();
            var item = includedItems.FirstOrDefault();

            return (item, resultVersion);
        }
    }
}
