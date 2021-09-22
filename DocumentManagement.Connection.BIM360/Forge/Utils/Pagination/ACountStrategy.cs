using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.Pagination
{
    public abstract class ACountStrategy<TPage> : IPaginationStrategy
        where TPage : class
    {
        private JToken lastResponse = null;

        protected abstract string PropertyName { get; }

        public void SetResponse(JToken response)
            => lastResponse = response;

        public IEnumerable<string> GetPages(string command)
        {
            var all = false;

            for (int i = 0; !all; i += Constants.ITEMS_ON_PAGE)
            {
                yield return SetPageParameters(command, Constants.ITEMS_ON_PAGE, i);

                var page = lastResponse[PropertyName]?.ToObject<TPage>();
                all = page == null || i + Constants.ITEMS_ON_PAGE >= GetCount(page);
            }
        }

        protected abstract int GetCount(TPage page);

        protected abstract string SetPageParameters(string command, int limit, int offset);
    }
}
