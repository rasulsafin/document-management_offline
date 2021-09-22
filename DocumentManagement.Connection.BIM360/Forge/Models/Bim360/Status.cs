using Brio.Docs.Connections.Bim360.Forge.Utils;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
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
        [EnumMember(Value = "work_completed")]
        WorkCompleted,
        [EnumMember(Value = "ready_to_inspect")]
        ReadyToInspect,
        [EnumMember(Value = "not_approved")]
        NotApproved,
        [EnumMember(Value = "in_dispute")]
        InDispute,
    }
}
