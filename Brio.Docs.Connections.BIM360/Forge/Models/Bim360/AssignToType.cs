using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum AssignToType
    {
        Undefined,
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "role")]
        Role,
        [EnumMember(Value = "company")]
        Company,
    }
}
