using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models
{
    public struct JsonApi
    {
        private JsonApi(string version)
            => Version = version;

        public static JsonApi Default
            => new JsonApi(Constants.JSON_API_VERSION);

        [DataMember(Name = "version")]
        public string Version { get; set; }
    }
}
