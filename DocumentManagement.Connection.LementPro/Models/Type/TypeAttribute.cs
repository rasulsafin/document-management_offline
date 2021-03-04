using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class TypeAttribute
    {
        [DataMember(Name = "isRequired")]
        public bool? IsRequired { get; set; }

        [DataMember(Name = "sortWeight")]
        public double? SortWeight { get; set; }

        [DataMember(Name = "field")]
        public string Field { get; set; }

        [DataMember(Name = "objectTypeId")]
        public int? ObjectTypeId { get; set; }

        [DataMember(Name = "attrId")]
        public int? AttrId { get; set; }

        [DataMember(Name = "childId")]
        public dynamic ChildId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "relationMultiplicity")]
        public int? RelationMultiplicity { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "dataType")]
        public int? DataType { get; set; }

        [DataMember(Name = "listOfValues")]
        public dynamic ListOfValues { get; set; }

        [DataMember(Name = "units")]
        public dynamic Units { get; set; }

        [DataMember(Name = "dontShowIfNull")]
        public bool? DontShowIfNull { get; set; }
    }
}
