using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), DM)]
    public enum UrnType
    {
        [EnumMember(Value = "oss")]
        Oss,
        [EnumMember(Value = "dm")]
        DM,
    }
}
