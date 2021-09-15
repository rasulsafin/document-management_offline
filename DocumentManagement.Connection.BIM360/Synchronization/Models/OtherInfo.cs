using System.Collections.Generic;
using System.Runtime.Serialization;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models
{
    [DataContract]
    internal class OtherInfo
    {
        [DataMember]
        public ICollection<BimElementExternalDto> BimElements { get; set; }

        [DataMember]
        public LinkedInfo OriginalModelInfo { get; set; }
    }
}
