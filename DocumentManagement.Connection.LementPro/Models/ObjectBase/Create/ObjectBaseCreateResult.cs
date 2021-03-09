using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBaseCreateResult
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "objectUrl")]
        public string ObjectUrl { get; set; }

        [DataMember(Name = "isRouteTask")]
        public bool? IsRouteTask { get; set; }

        [DataMember(Name = "isNew")]
        public bool? IsNew { get; set; }

        [DataMember(Name = "isObject")]
        public bool? IsObject { get; set; }

        [DataMember(Name = "isSuccess")]
        public bool? IsSuccess { get; set; }
    }
}
