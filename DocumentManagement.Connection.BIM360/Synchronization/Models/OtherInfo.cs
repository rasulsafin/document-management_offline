using Brio.Docs.Interface.Dtos;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Synchronization.Models
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
