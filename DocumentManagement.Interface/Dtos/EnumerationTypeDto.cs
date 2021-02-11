using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class EnumerationTypeDto
    {
        public int ID { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public ICollection<EnumerationValueDto> EnumerationValues { get; set; }
    }
}
