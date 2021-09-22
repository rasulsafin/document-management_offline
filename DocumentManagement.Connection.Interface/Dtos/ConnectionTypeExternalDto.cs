using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos
{
    public class ConnectionTypeExternalDto : IConnectionTypeDto
    {
        public string Name { get; set; }

        public IDictionary<string, string> AppProperties { get; set; }

        public IEnumerable<string> AuthFieldNames { get; set; }

        public ICollection<ObjectiveTypeExternalDto> ObjectiveTypes { get; set; }

        public ICollection<EnumerationTypeExternalDto> EnumerationTypes { get; set; }
    }
}
