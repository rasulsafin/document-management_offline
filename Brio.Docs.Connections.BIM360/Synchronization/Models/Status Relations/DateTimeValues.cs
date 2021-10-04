using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    internal enum DateTimeValues
    {
        Undefined,
        [EnumMember]
        Now,
    }
}
