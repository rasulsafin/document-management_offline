using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Authentication
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
