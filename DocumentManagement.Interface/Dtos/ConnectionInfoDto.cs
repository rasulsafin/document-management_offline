using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ConnectionInfoDto
    {
        public ID<ConnectionInfoDto> ID { get; set; }

        public ConnectionTypeDto ConnectionType { get; set; }

        public IEnumerable<string> AuthFieldNames { get; set; }
    }
}
