using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ConnectionTypeDto
    {
        public ID<ConnectionTypeDto> ID { get; set; }

        public string Name { get; set; }

        public IDictionary<string, string> AppProperties { get; set; }

        public IEnumerable<string> AuthFieldNames { get; set; }

        public ICollection<ObjectiveTypeDto> ObjectiveTypes { get; set; }

        public ICollection<EnumerationTypeDto> EnumerationTypes { get; set; }
    }
}
