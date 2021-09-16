using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
{
    internal static class IssueExtensions
    {
        public static OtherInfo GetOtherInfo(this Issue issue)
        {
            try
            {
                return ((JToken)issue?.Attributes?.PushpinAttributes?.ViewerState?.OtherInfo)?.ToObject<OtherInfo>();
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }
    }
}
