using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

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
            var length = arguments.Length;
            Array.Resize(ref arguments, length + 2);
            arguments[length++] = ITEMS_ON_PAGE;

            foreach (var argument in strategy.GetPageArguments())
            {
                arguments[length] = argument;
                var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    command,
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
