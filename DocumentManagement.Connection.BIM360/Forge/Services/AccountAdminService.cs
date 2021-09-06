using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination;
using MRS.DocumentManagement.Connection.Bim360.Properties;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class AccountAdminService
    {
        private readonly ForgeConnection connection;

        public AccountAdminService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<List<User>> GetAccountUsersAsync(string accountID)
        {
            var response = await connection.SendAsync(ForgeSettings.AppGet(), Resources.GetUsersMethodUS, accountID);
            return response.ToObject<List<User>>();
        }

        public async Task<List<ProjectUser>> GetProjectUsersAsync(string projectID)
            => await PaginationHelper.GetItemsByPages<ProjectUser, PaginationStrategy>(
                connection,
                Resources.GetProjectsUsersMethod,
                Constants.RESULTS_PROPERTY,
                projectID);

        public async Task<List<Role>> GetRolesAsync(string accountID, string projectID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetProjectsIndustryRolesMethodUS,
                accountID,
                projectID);
            return response.ToObject<List<Role>>();
        }

        public async Task<List<Company>> GetCompaniesAsync(string accountID, string projectID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AppGet(),
                Resources.GetProjectsCompaniesMethodUS,
                accountID,
                projectID);
            return response.ToObject<List<Company>>();
        }
    }
}
