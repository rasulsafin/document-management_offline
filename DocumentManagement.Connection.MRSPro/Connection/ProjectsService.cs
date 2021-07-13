using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectsService : Service
    {
        private static readonly string BASE_URL = "/project";

        public ProjectsService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        internal string RootPath => $"/{Auth.OrganizationId}{ROOT}";

        public async Task<IEnumerable<Project>> GetAll()
        {
            var listOfAllProjects = await GetListOfProjects();
            return listOfAllProjects.Where(p => p.Ancestry == RootPath).ToArray();
        }

        public async Task<IEnumerable<Project>> GetListOfProjects()
            => await HttpConnection.GetListOf<Project>(
                BASE_URL);

        public async Task<IEnumerable<Project>> TryGetByIds(IReadOnlyCollection<string> ids)
        {
            try
            {
                var idsStr = string.Join(QUERY_SEPARATOR, ids);
                return await HttpConnection.GetListOf<Project>(GetByIds(BASE_URL), idsStr);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Project> TryGetById(string id)
        {
            try
            {
                var res = await HttpConnection.GetListOf<Project>(GetByIds(BASE_URL), new[] { id });
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public Task<Project> TryPost(Project element)
        {
            throw new NotImplementedException();
        }

        public Task<Project> TryPatch(UpdatedValues valuesToPatch)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryDelete(string id)
        {
            throw new NotImplementedException();
        }
    }
}
