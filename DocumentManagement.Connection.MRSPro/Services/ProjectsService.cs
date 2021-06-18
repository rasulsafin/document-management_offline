using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectsService : Service, IElementService
    {
        private static readonly string BASE_URL = "/project";

        public ProjectsService(MrsProHttpConnection connection)
            : base(connection) { }

        private static string RootPath => $"/{Auth.OrganizationId}{ROOT}";

        public async Task<IEnumerable<Project>> GetRootProjects()
        {
            var listOfAllProjects = await HttpConnection.GetListOf<Project>(
                BASE_URL);

            return listOfAllProjects.Where(p => p.Ancestry == RootPath).ToArray();
        }

        public async Task<IEnumerable<IElement>> GetAll(DateTime date)
        {
            var listOfAllProjects = await HttpConnection.GetListOf<Project>(
                BASE_URL);

            return listOfAllProjects.Where(p => p.Ancestry != RootPath).ToArray();
        }

        public async Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids)
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

        public async Task<IElement> TryGetById(string id)
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

        public Task<IElement> TryPost(IElement element)
        {
            throw new NotImplementedException();
        }

        public Task<IElement> TryPatch(UpdatedValues valuesToPatch)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryDelete(string id)
        {
            throw new NotImplementedException();
        }
    }
}
