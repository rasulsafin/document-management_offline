using System;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Extensions;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    internal static class ScopeUtilities
    {
        public static string GetScopeString(params Enum[] scopes)
            => string.Join(' ', scopes.Select(x => x.GetEnumMemberValue()));
    }
}
