using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    /// <summary>
    /// The service for getting users.
    /// </summary>
    public interface IUsersGetter
    {
        /// <summary>
        /// Query all the users in a specific BIM 360 account.
        /// </summary>
        /// <param name="hub"> The hub of the users.</param>
        /// <returns>All the users in a specific BIM 360 account.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/users-GET/">`<b>GET</b> users` on forge.autodesk.com</a></footer>
        Task<List<User>> GetAccountUsersAsync(Hub hub);

        /// <summary>
        /// Retrieves information about all the users in a project.
        /// To get information about all the users in an account, see <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/users-GET/">GET accounts/users</a>.
        /// </summary>
        /// <param name="projectID">The project ID of the users.</param>
        /// <returns>The information about all the users in a project</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/admin-v1-projects-projectId-users-GET/">`<b>GET</b> projects/:projectId/users` on forge.autodesk.com</a></footer>
        IAsyncEnumerable<ProjectUser> GetProjectUsersAsync(string projectID);
    }
}
