using System.Runtime.Serialization;

namespace Brio.Docs.Connection.MrsPro.Models
{
    [DataContract]
    public class IssueExtraInfo
    {
        [DataMember(Name = "hasComment")]
        public bool HasComment { get; set; }

        [DataMember(Name = "hasImage")]
        public bool HasImage { get; set; }

        [DataMember(Name = "hasReport")]
        public bool HasReport { get; set; }

        [DataMember(Name = "taskId")]
        public string TaskId { get; set; }
    }
}
