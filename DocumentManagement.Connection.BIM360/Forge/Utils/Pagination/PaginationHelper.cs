using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connection.Bim360.Forge.Utils.Pagination
{
    public static class PaginationHelper
    {
        public static async Task<List<TResult>> GetItemsByPages<TResult, TPaginationStrategy>(
            ForgeConnection connection,
            string command,
            string itemsProperty,
            params object[] arguments)
            where TPaginationStrategy : IPaginationStrategy, new()
            => await GetItemsByPages<TResult, TPaginationStrategy>(
                connection,
                command,
                response => response[itemsProperty]?.ToObject<IEnumerable<TResult>>() ?? ArraySegment<TResult>.Empty,
                arguments);

        public static async Task<List<TResult>> GetItemsByPages<TResult, TPaginationStrategy>(
            ForgeConnection connection,
            ForgeSettings forgeSettings,
            string command,
            string itemsProperty,
            params object[] arguments)
            where TPaginationStrategy : IPaginationStrategy, new()
            => await GetItemsByPages<TResult, TPaginationStrategy>(
                connection,
                forgeSettings,
                command,
                response => response[itemsProperty]?.ToObject<IEnumerable<TResult>>() ?? ArraySegment<TResult>.Empty,
                arguments);

        public static async Task<List<TResult>> GetItemsByPages<TResult, TPaginationStrategy>(
            ForgeConnection connection,
            string command,
            Func<JToken, IEnumerable<TResult>> convert,
            params object[] arguments)
            where TPaginationStrategy : IPaginationStrategy, new()
            => await GetItemsByPages<TResult, TPaginationStrategy>(
                connection,
                ForgeSettings.AuthorizedGet(),
                command,
                convert,
                arguments);

        public static async Task<List<TResult>> GetItemsByPages<TResult, TPaginationStrategy>(
            ForgeConnection connection,
            ForgeSettings forgeSettings,
            string command,
            Func<JToken, IEnumerable<TResult>> convert,
            params object[] arguments)
            where TPaginationStrategy : IPaginationStrategy, new()
        {
            var strategy = new TPaginationStrategy();
            var result = new List<TResult>();
            forgeSettings ??= ForgeSettings.AuthorizedGet();

            foreach (var page in strategy.GetPages(command))
            {
                var response = await connection.SendAsync(
                    forgeSettings,
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
