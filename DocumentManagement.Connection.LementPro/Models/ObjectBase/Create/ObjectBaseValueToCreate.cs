using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBaseValueToCreate
    {
        [DataMember(Name = "type")]
        public int? Type { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "project")]
        public string Project { get; set; }

        [DataMember(Name = "bimRef")]
        public string BimRef { get; set; }

        /// <summary>
        /// ??? Response contains the field.
        /// During creating this field should contain user id (web form uses logged in user id).
        /// </summary>
        [DataMember(Name = "i60099")]
        public string I60099 { get; set; }

        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        [DataMember(Name = "isExpired")]
        public bool? IsExpired { get; set; }

        [DataMember(Name = "isResolution")]
        public bool? IsResolution { get; set; }
    }
}
