using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Extension
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; } = Constants.DEFAULT_EXTENSION_VERSION;
    }
}
