using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.Pagination
{
    public class LinksStrategy : IPaginationStrategy
    {
        private JToken lastResponse = null;

        public void SetResponse(JToken response)
            => lastResponse = response;

        public IEnumerable<string> GetPages(string command)
        {
            var all = false;

            for (int i = 0; !all; i++)
            {
                yield return ForgeConnection.SetParameter(command, PageFilter.ByNumber(Constants.ITEMS_ON_PAGE, i));

                var next = lastResponse[Constants.LINKS_PROPERTY]?["next"]?["href"];
                all = string.IsNullOrWhiteSpace(next?.Value<string>());
            }
        }
    }
}
