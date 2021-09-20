using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Transient)]
    public enum OssRetentionPolicy
    {
        [EnumMember(Value = "transient")]
        Transient,
        [EnumMember(Value = "temporary")]
        Temporary,
        [EnumMember(Value = "persistent")]
        Persistent,
    }
}
