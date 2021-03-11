using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ConnectionInfoExternalDto : IConnectionInfoDto
    {
        public ConnectionTypeExternalDto ConnectionType { get; set; }

        public IDictionary<string, string> AuthFieldValues { get; set; }

        public ICollection<EnumerationTypeExternalDto> EnumerationTypes { get; set; }
    }
}
