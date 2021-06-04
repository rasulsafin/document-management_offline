using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectService : Service
    {
        public ProjectService(MrsProHttpConnection connection)
            : base(connection) { }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            return await Connector.SendAsync<IEnumerable<Project>>(HttpMethod.Get, URLs.GetProjects);
        }

        //public async Task<ProjectDto> GetProject(string id)
        //{

        //}
    }
}
