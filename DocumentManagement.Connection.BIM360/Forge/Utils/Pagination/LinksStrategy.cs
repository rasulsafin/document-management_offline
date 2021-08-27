using System.Collections;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination
{
    public class LinksStrategy : IPaginationStrategy
    {
        private JToken lastResponse = null;

        public void SetResponse(JToken response)
            => lastResponse = response;

        public IEnumerable GetPageArguments()
        {
            var all = false;

            for (int i = 0; !all; i++)
            {
                yield return i;

                var next = lastResponse[Constants.LINKS_PROPERTY]?["next"]?["href"];
                all = string.IsNullOrWhiteSpace(next?.Value<string>());
            }
        }
    }
}
