using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class LementProProject
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "urlPart")]
        public string UrlPart { get; set; }

        [DataMember(Name = "categoryId")]
        public int? CategoryId { get; set; }

        [DataMember(Name = "isClosed")]
        public bool? IsClosed { get; set; }
    }
}
