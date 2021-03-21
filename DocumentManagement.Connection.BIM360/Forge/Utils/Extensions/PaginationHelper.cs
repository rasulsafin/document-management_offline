using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions
{
    public static class PaginationHelper
    {
        public static async Task<List<T>> GetItemsByPages<T>(
            ForgeConnection connection,
            string command,
            params object[] arguments)
        {
            var result = new List<T>();
            var all = false;
            var length = arguments.Length;
            Array.Resize(ref arguments, length + 2);
            arguments[length++] = ITEMS_ON_PAGE;

            for (int i = 0; !all; i++)
            {
                arguments[length] = i;
                var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    command,
                    arguments);
                var data = response[DATA_PROPERTY]?.ToObject<List<T>>();
                if (data != null)
                    result.AddRange(data);
                var meta = response[META_PROPERTY]?.ToObject<Meta>();
                all = meta == null || meta.Page.Limit * ((meta.Page.Offset / ITEMS_ON_PAGE) + 1) >= meta.RecordCount;
            }

            return result;
        }
    }
}
