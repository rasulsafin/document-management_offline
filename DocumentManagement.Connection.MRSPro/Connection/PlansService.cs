using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class PlansService : Service
    {
        private static readonly string BASE_URL = "/plan";
        private static readonly string BASE_NAME = "file";

        public PlansService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        internal async Task<IEnumerable<Plan>> TryGetByProjectIdAsync(string projectId)
        {
            try
            {
                var res = await HttpConnection.GetListOf<Plan>(BASE_URL);
                return res.Where(x => x.Ancestry.Contains(projectId));
            }
            catch
            {
                return null;
            }
        }

        internal async Task<IEnumerable<Plan>> TryGetByParentIdAsync(string parentId)
        {
            try
            {
                var res = await HttpConnection.GetListOf<Plan>(GetByParentPath(BASE_URL), parentId);
                return res;
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Uri> GetPlanUriAsync(string id,
            string parentId)
        {
            string query = $"?ids={id}&parentId={parentId}&tokenOnly=false&type=plan";
            var uri = await HttpConnection.GetUri(BASE_URL + query);

            return uri;
        }

        internal async Task<Plan> TryPatchAsync(UpdatedValues valuesToPatch)
        {
            try
            {
                var result = await HttpConnection.PatchJson<IEnumerable<Plan>, UpdatedValues>(BASE_URL, valuesToPatch);
                return result.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        internal async Task<bool> TryDeleteAsync(string id)
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

        internal async Task<bool> TryUploadAsync(Plan plan,
            string originalName,
            byte[] file,
            string folderId)
        {
            try
            {
                await HttpConnection.PostJson<Plan>(BASE_URL, plan, file, originalName, folderId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
