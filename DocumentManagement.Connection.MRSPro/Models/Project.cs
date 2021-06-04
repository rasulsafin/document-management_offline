using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class Project
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "ancestry")]
        public string Ancestry { get; set; }

        [DataMember(Name = "createdDate")]
        public long CreatedDate { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "parentType")]
        public string ParentType { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }
    }
}
