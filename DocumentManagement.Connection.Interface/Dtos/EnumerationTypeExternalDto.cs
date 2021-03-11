using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class EnumerationTypeExternalDto
    {
        public string ExternalID { get; set; }

        public string Name { get; set; }

        public ICollection<EnumerationValueExternalDto> EnumerationValues { get; set; }
    }
}