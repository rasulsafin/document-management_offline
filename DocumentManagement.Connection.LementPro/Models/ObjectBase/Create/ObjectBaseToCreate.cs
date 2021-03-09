using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBaseToCreate
    {
        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "canAutoEditParents")]
        public bool? CanAutoEditParents { get; set; }

        [DataMember(Name = "canAutoEditEndDate")]
        public bool? CanAutoEditEndDate { get; set; }

        [DataMember(Name = "values")]
        public ObjectBaseValueToCreate Values { get; set; }

        [DataMember(Name = "fileIds")]
        public IEnumerable<int> FileIds { get; set; }
    }
}
