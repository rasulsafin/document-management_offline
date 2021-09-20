using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Read)]
    public enum BucketAccess
    {
        [EnumMember(Value = "full")]
        Full,
        [EnumMember(Value = "read")]
        Read,
    }
}
