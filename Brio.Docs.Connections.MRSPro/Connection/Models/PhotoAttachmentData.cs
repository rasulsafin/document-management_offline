using System.Runtime.Serialization;

namespace Brio.Docs.Connections.MrsPro.Models
{
    [DataContract]
    public class PhotoAttachmentData
    {
        [DataMember(Name = "file")]
        public string File { get; set; }
    }
}
