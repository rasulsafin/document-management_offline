using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum Region
    {
        Undefined,
        [EnumMember(Value = "US")]
        US,
        [EnumMember(Value = "EMEA")]
        Emea,
    }
}