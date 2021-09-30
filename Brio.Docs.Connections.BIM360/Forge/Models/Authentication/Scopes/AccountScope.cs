using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication.Scopes
{
    internal enum AccountScope
    {
        [EnumMember(Value = "account:read")]
        Read,
        [EnumMember(Value = "account:write")]
        Write,
    }
}
