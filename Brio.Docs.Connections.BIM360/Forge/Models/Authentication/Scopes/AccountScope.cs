using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Authentication.Scopes
{
    internal enum AccountScope
    {
        [EnumMember(Value = "account:read")]
        Read,
        [EnumMember(Value = "account:write")]
        Write,
    }
}
