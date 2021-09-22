using System.Runtime.Serialization;

namespace Brio.Docs.Connection.LementPro.Models
{
    [DataContract]
    public class UserShortInfo
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "isInVacation")]
        public bool? IsInVacation { get; set; }

        [DataMember(Name = "avatarFileId")]
        public string AvatarFileId { get; set; }
    }
}
