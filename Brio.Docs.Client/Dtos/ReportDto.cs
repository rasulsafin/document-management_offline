using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Dtos
{
    public class ReportDto
    {
        [Required(ErrorMessage = "ValidationError_ObjectivesIsRequired")]
        public IEnumerable<ID<ObjectiveDto>> Objectives { get; set; }

        public string Location { get; set; }

        public string Address { get; set; }

        public string ObjectName { get; set; }

        public string ReviewerPosition { get; set; }

        public string ReviewerName { get; set; }

        public string ReviewerCompany { get; set; }

        public string ResponsiblePosition { get; set; }

        public string ResponsibleName { get; set; }

        public string ResponsibleCompany { get; set; }
    }
}
