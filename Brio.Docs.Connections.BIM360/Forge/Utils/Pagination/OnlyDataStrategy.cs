using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.Pagination
{
    public class OnlyDataStrategy : IPaginationStrategy
    {
        private JToken lastResponse = null;

        public void SetResponse(JToken response)
            => lastResponse = response;

        public IEnumerable<string> GetPages(string command)
        {
            var all = false;

            for (int i = 0; !all; i += Constants.ITEMS_ON_PAGE)
            {
                yield return ForgeConnection.SetParameters(
                    command,
                    new IQueryParameter[]
                    {
                        new QueryParameter(Constants.LIMIT_PARAMETER_NAME, Constants.ITEMS_ON_PAGE.ToString()),
                        new QueryParameter(Constants.OFFSET_PARAMETER_NAME, i.ToString()),
                    });

                all = !lastResponse.HasValues;
            }
        }
    }
}
