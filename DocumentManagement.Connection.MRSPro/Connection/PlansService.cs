using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class PlansService : Service
    {
        private static readonly string BASE_URL = "/plan";
        private static readonly string BASE_NAME = "file";

        public PlansService(MrsProHttpConnection connection)
            : base(connection) { }

        internal async Task<IEnumerable<Plan>> TryGetByParentId(string parentId)
        {
            try
            {
                var url = BASE_URL + "?ancestry={0}";
                var res = await HttpConnection.GetListOf<Plan>(url, parentId);
                return res;
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Plan> TryPost(string filePath)
        {
            try
            {
                using FileStream fileStream = File.OpenRead(filePath);
                //var result = await HttpConnection.PostFormData<Plan>(BASE_URL, fileStream, BASE_NAME);
                //return result;
                return null;
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Plan> TryPatch(UpdatedValues valuesToPatch)
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
    }
}
