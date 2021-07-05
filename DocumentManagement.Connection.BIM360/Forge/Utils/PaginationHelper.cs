using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using Newtonsoft.Json.Linq;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public static class PaginationHelper
    {
        public static async Task<List<T>> GetItemsByPages<T>(
            ForgeConnection connection,
            string command,
            params object[] arguments)
        {
            return await GetItemsByPages(
                connection,
                command,
                response => response[DATA_PROPERTY]?.ToObject<IEnumerable<T>>() ?? ArraySegment<T>.Empty,
                arguments);
        }

        public static async Task<List<T>> GetItemsByPages<T>(
            ForgeConnection connection,
            string command,
            Func<JToken, IEnumerable<T>> convert,
            params object[] arguments)
        {
            var result = new List<T>();
            var all = false;
            var length = arguments.Length;
            Array.Resize(ref arguments, length + 2);
            arguments[length++] = ITEMS_ON_PAGE;

            for (int i = 0; !all; i += ITEMS_ON_PAGE)
            {
                arguments[length] = i;
                var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    command,
                    arguments);
                var data = convert(response);
                if (data != null)
                    result.AddRange(data);
                var meta = response[META_PROPERTY]?.ToObject<Meta>();
                all = meta == null || i + ITEMS_ON_PAGE >= meta.RecordCount;
            }

            return result;
        }
    }
}
