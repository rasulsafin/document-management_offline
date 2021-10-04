using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum ComparisonObjectType
    {
        Undefined,
        [EnumMember(Value = "BRIO MRS")]
        BrioMrs,
        [EnumMember(Value = "BIM 360")]
        Bim360,
    }
}
