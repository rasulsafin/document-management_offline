using Brio.Docs.Connection.Bim360.Forge.Utils;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Brio.Docs.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    [JsonConverter(typeof(SafeStringEnumConverter), Undefined)]
    public enum Region
    {
        Undefined,
        US,
        Emea,
    }
}
