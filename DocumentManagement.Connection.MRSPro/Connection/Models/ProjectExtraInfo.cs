using System.Runtime.Serialization;

namespace Brio.Docs.Connection.MrsPro.Models
{
    [DataContract]
    public class ProjectExtraInfo
    {
        [DataMember(Name = "hasDocumentation")]
        public bool HasDocumentation { get; set; }

        [DataMember(Name = "projectId")]
        public string ProjectId { get; set; }

        [DataMember(Name = "stateCounter")]
        public object StateCounter { get; set; }
    }
}
