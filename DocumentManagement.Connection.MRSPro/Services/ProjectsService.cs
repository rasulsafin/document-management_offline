using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectsService : Service, IElementService
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

        public async Task<IEnumerable<IElement>> GetAll(DateTime date)
        {
            var listOfAllProjects = await HttpConnection.GetAll<Project>(
                URLs.GetProjects);

            return listOfAllProjects.Where(p => p.Ancestry != RootPath).ToArray();
        }

        public async Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids)
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

        public async Task<IElement> TryGetById(string id)
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

        public Task<IElement> TryPost(IElement element)
        {
            throw new NotImplementedException();
        }

        public Task<IElement> TryPatch(UpdatedValues valuesToPatch)
        {
            throw new NotImplementedException();
        }
    }
}
