using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ConnectionTypeDto
    {
        public ID<ConnectionTypeDto> ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> AuthFieldNames { get; set; }

    }
}
