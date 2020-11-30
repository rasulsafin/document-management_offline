using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class RemoteConnectionInfoDto
    {
        public ID<RemoteConnectionInfoDto> ID { get; set; }
        public string ServiceName { get; set; }
        public IEnumerable<string> AuthFieldNames { get; set; }
    }
}
