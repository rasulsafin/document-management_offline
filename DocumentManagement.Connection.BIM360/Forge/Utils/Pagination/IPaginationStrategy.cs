using System.Collections;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination
{
    public interface IPaginationStrategy
    {
        void SetResponse(JToken response);

        IEnumerable GetPageArguments();
    }
}
