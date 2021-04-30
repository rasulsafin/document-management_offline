using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers
{
    public class HubsHelper
    {
        private readonly HubsService hubsService;

        public HubsHelper(HubsService hubsService)
            => this.hubsService = hubsService;

        public async Task<Hub> GetDefaultHubAsync()
            => (await hubsService.GetHubsAsync()).FirstOrDefault();
    }
}
