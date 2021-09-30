using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum OssRetentionPolicy
    {
        Undefined,
        [EnumMember(Value = "transient")]
        Transient,
        [EnumMember(Value = "temporary")]
        Temporary,
        [EnumMember(Value = "persistent")]
        Persistent,
    }
}
