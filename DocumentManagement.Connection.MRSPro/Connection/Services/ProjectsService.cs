using Brio.Docs.Connections.MrsPro.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro.Services
{
    public class ProjectsService : Service
    {
        private static readonly string BASE_URL = "/project";
        private static readonly string BASE_EXTRA_URL = $"/extra{BASE_URL}";
        private readonly PlansService plansService;

        public ProjectsService(MrsProHttpConnection connection, PlansService plansService)
            : base(connection)
        {
            this.plansService = plansService;
        }

        internal string RootPath => $"/{Auth.OrganizationId}{ROOT}";

        internal async Task<IEnumerable<Project>> GetAll()
            => await HttpConnection.GetListOf<Project>(
                BASE_URL);

        internal async Task<IEnumerable<Project>> TryGetByIds(IReadOnlyCollection<string> ids)
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

        internal async Task<Project> TryGetById(string id)
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

        internal async Task<Project> TryPost(Project element)
        {
            try
            {
                var result = await HttpConnection.PostJson<Project>(BASE_URL, element);
                return result;
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Project> TryPatch(UpdatedValues valuesToPatch)
        {
            try
            {
                var result = await HttpConnection.PatchJson<IEnumerable<Project>, UpdatedValues>(BASE_URL, valuesToPatch);
                return result.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        internal async Task<bool> TryDelete(string id)
        {
            try
            {
                await HttpConnection.DeleteJson(BASE_URL, new[] { id });
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal async Task<IEnumerable<Plan>> GetAttachments(string ancestry)
        {
            var result = await plansService.TryGetByParentIdAsync(ancestry);
            return result;
        }

        internal async Task<IEnumerable<ProjectExtraInfo>> TryGetAttachmentInfoByIds(IReadOnlyCollection<string> ids)
        {
            try
            {
                var idsStr = string.Join(QUERY_SEPARATOR, ids);
                return await HttpConnection.GetListOf<ProjectExtraInfo>(GetByIds(BASE_EXTRA_URL), idsStr);
            }
            catch
            {
                return null;
            }
        }
    }
}
