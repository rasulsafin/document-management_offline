using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBaseToUpdate
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "canAutoEditParents")]
        public bool? CanAutoEditParents { get; set; }

        [DataMember(Name = "values")]
        public ObjectBaseValueToUpdate Values { get; set; }

        [DataMember(Name = "addedFileIds")]
        public List<string> AddedFileIds { get; set; }

        [DataMember(Name = "removedFileIds")]
        public List<string> RemovedFileIds { get; set; }
    }
}
