using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {
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
