using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360
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
