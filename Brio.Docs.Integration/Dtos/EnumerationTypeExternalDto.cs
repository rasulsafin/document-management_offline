using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos
{
    public class EnumerationTypeExternalDto
    {
        public string ExternalID { get; set; }

        public string Name { get; set; }

        public ICollection<EnumerationValueExternalDto> EnumerationValues { get; set; }
    }
}