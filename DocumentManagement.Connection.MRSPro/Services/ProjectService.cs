using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectService : Service
    {
        public ProjectService(MrsProHttpConnection connection)
            : base(connection) { }

        internal async Task<IEnumerable<Project>> GetRootProjects()
        {
            // TODO: Cache? 
            var listOfAllProjects = await HttpConnection.SendAsync<IEnumerable<Project>>(
                HttpMethod.Get,
                URLs.GetProjects);

            return listOfAllProjects.Where(p => p.Ancestry == $"/{Auth.OrganizationId}{Constants.ROOT}").ToArray();
        }

        internal async Task<IEnumerable<Project>> GetProjects()
        {
                var listOfAllProjects = await HttpConnection.SendAsync<IEnumerable<Project>>(
                    HttpMethod.Get,
                    URLs.GetProjects);

                return listOfAllProjects.Where(p => p.Ancestry != $"/{Auth.OrganizationId}{Constants.ROOT}").ToArray();
        }

        internal async Task<IEnumerable<Project>> GetProjectsById(IReadOnlyCollection<string> ids)
        {
            return await GetById<IEnumerable<Project>>(GetValueString(ids), URLs.GetProjectsByIds);
        }

        internal async Task<Project> TryGetProjectById(string id)
        {
            try
            {
                var res = await GetById<IEnumerable<Project>>(id, URLs.GetProjectsByIds);
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
