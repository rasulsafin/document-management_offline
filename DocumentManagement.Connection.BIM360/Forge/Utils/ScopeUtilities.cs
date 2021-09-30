using System;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    internal static class ScopeUtilities
    {
        public static string GetScopeString(params Enum[] scopes)
            => string.Join(' ', scopes.Select(x => x.GetEnumMemberValue()));
    }
}
