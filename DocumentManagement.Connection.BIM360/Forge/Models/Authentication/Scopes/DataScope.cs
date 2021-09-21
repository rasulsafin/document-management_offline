using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication.Scopes
{
    public enum DataScope
    {
        [EnumMember(Value = "data:read")]
        Read,
        [EnumMember(Value = "data:write")]
        Write,
        [EnumMember(Value = "data:create")]
        Create,
        [EnumMember(Value = "data:search")]
        Search,
    }
}
