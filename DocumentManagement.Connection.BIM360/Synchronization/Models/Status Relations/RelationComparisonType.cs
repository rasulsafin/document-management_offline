using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    internal enum RelationComparisonType
    {
        Undefined,
        [EnumMember]
        Equal,
        [EnumMember]
        NotEqual,
        [EnumMember]
        Greater,
        [EnumMember]
        Less,
    }
}
