using System.Collections.Generic;
using System.Runtime.Serialization;
using Brio.Docs.Integration.Dtos;

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
