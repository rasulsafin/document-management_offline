using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum UrnType
    {
        Undefined,
        [EnumMember(Value = "oss")]
        Oss,
        [EnumMember(Value = "dm")]
        DM,
    }
}
