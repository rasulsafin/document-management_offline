using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.Pagination
{
    /// <summary>
    /// Represents pagination strategies for BIM 360 queries.
    /// </summary>
    public interface IPaginationStrategy
    {
        /// <summary>
        /// Sets a page response.
        /// </summary>
        /// <param name="response">The page of the last response.</param>
        void SetResponse(JToken response);

        /// <summary>
        /// Gets command with page parameters.
        /// </summary>
        /// <param name="command">The Forge API command.</param>
        /// <returns>The enumeration of page commands.</returns>
        IEnumerable<string> GetPages(string command);
    }
}
