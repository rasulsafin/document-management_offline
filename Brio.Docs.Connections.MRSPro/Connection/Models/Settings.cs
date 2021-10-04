using System.Runtime.Serialization;

namespace Brio.Docs.Connections.MrsPro.Models
{
    [DataContract]
    public class Settings
    {
        [DataMember(Name = "pushNotify")]
        public bool PushNotify { get; set; }

        [DataMember(Name = "emailNotify")]
        public bool EmailNotify { get; set; }

        [DataMember(Name = "notifyType")]
        public string NotifyType { get; set; }

        [DataMember(Name = "geolocation")]
        public bool Geolocation { get; set; }

        [DataMember(Name = "tfaType")]
        public string TfaType { get; set; }
    }
}
