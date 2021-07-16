﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectsService : Service
    {
        private static readonly string BASE_URL = "/project";
        private readonly PlansService plansService;

        public ProjectsService(MrsProHttpConnection connection, PlansService plansService)
            : base(connection)
        {
            this.plansService = plansService;
        }

        internal string RootPath => $"/{Auth.OrganizationId}{ROOT}";

        internal async Task<IEnumerable<Project>> GetAll()
        {
            var listOfAllProjects = await GetListOfProjects();
            return listOfAllProjects.Where(p => p.Ancestry == RootPath).ToArray();
        }

        internal async Task<IEnumerable<Project>> GetListOfProjects()
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
            var result = await plansService.TryGetByParentId(ancestry);
            return result;
        }
    }
}
