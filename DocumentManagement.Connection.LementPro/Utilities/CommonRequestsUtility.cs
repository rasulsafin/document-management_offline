using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;

namespace MRS.DocumentManagement.Connection.LementPro.Utilities
{
    public class CommonRequestsUtility
    {
        public CommonRequestsUtility()
        {
        }

        public CommonRequestsUtility(HttpRequestUtility requestUtility)
            => RequestUtility = requestUtility;

        protected virtual HttpRequestUtility RequestUtility { get; set; }

        protected internal async Task<List<Category>> GetMenuCategoriesAsync(string token)
        {
            var response = await RequestUtility.GetResponseAsync(Resources.MethodGetMenuCategories, data: (object)null);
            return response.ToObject<List<Category>>();
        }
    }
}
