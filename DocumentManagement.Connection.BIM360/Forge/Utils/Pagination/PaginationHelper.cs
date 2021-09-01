using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Pagination
{
    public static class PaginationHelper
    {
        public static async Task<List<TResult>> GetItemsByPages<TResult, TPaginationStrategy>(
            ForgeConnection connection,
            string command,
            string itemsProperty,
            params object[] arguments)
            where TPaginationStrategy : IPaginationStrategy, new()
        {
            return await GetItemsByPages<TResult, TPaginationStrategy>(
                connection,
                command,
                response => response[itemsProperty]?.ToObject<IEnumerable<TResult>>() ?? ArraySegment<TResult>.Empty,
                arguments);
        }

        public static async Task<List<TResult>> GetItemsByPages<TResult, TPaginationStrategy>(
            ForgeConnection connection,
            string command,
            Func<JToken, IEnumerable<TResult>> convert,
            params object[] arguments)
            where TPaginationStrategy : IPaginationStrategy, new()
        {
            var strategy = new TPaginationStrategy();
            var result = new List<TResult>();

            foreach (var page in strategy.GetPages(command))
            {
                var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    page,
                    arguments);
                var data = convert(response);
                if (data != null)
                    result.AddRange(data);
                strategy.SetResponse(response);
            }

            return result;
        }
    }
}
