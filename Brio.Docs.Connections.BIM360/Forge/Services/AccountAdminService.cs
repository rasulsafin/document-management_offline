using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Forge.Utils.Pagination;
using Brio.Docs.Connections.Bim360.Properties;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class AccountAdminService : IUsersGetter
    {
        private readonly ForgeConnection connection;

        public AccountAdminService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<List<User>> GetAccountUsersAsync(Hub hub)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AppGet(),
                hub.IsEmea() ? Resources.GetUsersMethodEmea : Resources.GetUsersMethodUS,
                hub.GetAccountID());
            return response.ToObject<List<User>>();
        }

        public async Task<List<ProjectUser>> GetProjectUsersAsync(string projectID)
            => await PaginationHelper.GetItemsByPages<ProjectUser, PaginationStrategy>(
                connection,
                Resources.GetProjectsUsersMethod,
                Constants.RESULTS_PROPERTY,
                projectID);

        public async Task<List<Role>> GetRolesAsync(Hub hub, string projectID)
        {
            var projectsIndustryRolesMethod = hub.IsEmea()
                ? Resources.GetProjectsIndustryRolesMethodEmea
                : Resources.GetProjectsIndustryRolesMethodUS;
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                projectsIndustryRolesMethod,
                hub.GetAccountID(),
                projectID);
            return response.ToObject<List<Role>>();
        }

        public async Task<List<Company>> GetCompaniesAsync(Hub hub, string projectID)
        {
            var projectsCompaniesMethod = hub.IsEmea()
                ? Resources.GetProjectsCompaniesMethodEmea
                : Resources.GetProjectsCompaniesMethodUS;
            return await PaginationHelper.GetItemsByPages<Company, OnlyDataStrategy>(
                connection,
                ForgeSettings.AppGet(),
                projectsCompaniesMethod,
                token => token.ToObject<List<Company>>(),
                hub.GetAccountID(),
                projectID);
        }
    }
}
