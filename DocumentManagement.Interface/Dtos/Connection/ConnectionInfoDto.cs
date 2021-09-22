using System.Collections.Generic;

namespace Brio.Docs.Interface.Dtos
{
    public class ConnectionInfoDto : IConnectionInfoDto
    {
        public ID<ConnectionInfoDto> ID { get; set; }

        public ConnectionTypeDto ConnectionType { get; set; }

        public IDictionary<string, string> AuthFieldValues { get; set; }

        public ICollection<EnumerationTypeDto> EnumerationTypes { get; set; }
    }
}
