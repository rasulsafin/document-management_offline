using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectsService : Service
    {
        public ProjectsService(MrsProHttpConnection connection)
            : base(connection) { }

        private static string RootPath => $"/{Auth.OrganizationId}{Constants.ROOT}";

        public async Task<IEnumerable<Project>> GetRootProjects()
        {
            // TODO: Cache?
            var listOfAllProjects = await HttpConnection.GetAll<Project>(
                URLs.GetProjects);

            return listOfAllProjects.Where(p => p.Ancestry == RootPath).ToArray();
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var listOfAllProjects = await HttpConnection.GetAll<Project>(
                URLs.GetProjects);

            return listOfAllProjects.Where(p => p.Ancestry != RootPath).ToArray();
        }

        public async Task<IEnumerable<Project>> TryGetProjectsById(IReadOnlyCollection<string> ids)
        {
            try
            {
                return await HttpConnection.GetByIds<Project>(URLs.GetProjectsByIds, ids);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Project> TryGetProjectById(string id)
        {
            try
            {
                var res = await HttpConnection.GetByIds<Project>(URLs.GetProjectsByIds, new[] { id });
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
