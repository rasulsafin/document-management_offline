using System.Runtime.Serialization;

namespace Brio.Docs.Connection.LementPro.Models
{
    [DataContract]
    public class Category
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "sortOrder")]
        public int? SortOrder { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
