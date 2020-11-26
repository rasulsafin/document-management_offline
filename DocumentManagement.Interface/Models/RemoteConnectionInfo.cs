using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Models
{
    public class RemoteConnectionInfo
    {
        public ID<RemoteConnectionInfo> ID { get; set; }
        public string ServiceName { get; set; }
        public IEnumerable<string> AuthFieldNames { get; set; }
    }
}
