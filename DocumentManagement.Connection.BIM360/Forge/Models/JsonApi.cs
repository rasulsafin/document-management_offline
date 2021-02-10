using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models
{
    [DataContract]
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
