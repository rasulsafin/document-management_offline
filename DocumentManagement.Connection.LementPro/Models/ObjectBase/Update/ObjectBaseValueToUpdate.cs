using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBaseValueToUpdate
    {
        [DataMember(Name = "type")]
        public int? Type { get; set; }

        [DataMember(Name = "lastModifiedDate")]
        public string LastModifiedDate { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "project")]
        public int? Project { get; set; }

        [DataMember(Name = "bimRef")]
        public int? BimRef { get; set; }

        [DataMember(Name = "id")]
        public int? ID { get; set; }

        /// <summary>
        /// ??? Response contains the field.
        /// During creating this field should contain user id (web form uses logged in user id).
        /// </summary>
        [DataMember(Name = "i60099")]
        public int? I60099 { get; set; }

        [DataMember(Name = "documentResolutionFiles")]
        public dynamic DocumentResolutionFiles { get; set; }

        [DataMember(Name = "creationDate")]
        public string CreationDate { get; set; }

        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        [DataMember(Name = "isResolution")]
        public bool? IsResolution { get; set; }

        [DataMember(Name = "managers")]
        public int? Managers { get; set; }

        [DataMember(Name = "controllers")]
        public string Controllers { get; set; }

        [DataMember(Name = "executors")]
        public string Executors { get; set; }

        [DataMember(Name = "isExpired")]
        public bool? IsExpired { get; set; }

        [DataMember(Name = "favorites")]
        public string Favorites { get; set; }

        [DataMember(Name = "members")]
        public string Members { get; set; }
    }
}
