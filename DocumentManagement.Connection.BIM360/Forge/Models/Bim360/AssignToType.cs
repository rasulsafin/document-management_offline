using Brio.Docs.Connection.Bim360.Forge.Utils;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Brio.Docs.Connection.Bim360.Forge.Models.Bim360
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), None)]
    public enum AssignToType
    {
        [EnumMember(Value = null)]
        None,
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "role")]
        Role,
        [EnumMember(Value = "company")]
        Company,
    }
}
