using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum Status
    {
        Undefined,
        [EnumMember(Value = "draft")]
        Draft,
        [EnumMember(Value = "open")]
        Open,
        [EnumMember(Value = "closed")]
        Closed,
        [EnumMember(Value = "void")]
        Void,
        [EnumMember(Value = "answered")]
        Answered,
    }
}
