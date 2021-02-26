using System;
using System.Collections.Generic;
using System.Text;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ObjectiveExternalDto
    {
        public string ExternalID { get; set; }

        public string ProjectExternalID { get; set; }

        public string ParentObjectiveExternalID { get; set; }

        public string AuthorExternalID { get; set; }

        public ObjectiveTypeExternalDto ObjectiveType { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public ICollection<ItemExternalDto> Items { get; set; }

        public ICollection<DynamicFieldExternalDto> DynamicFields { get; set; }

        public ICollection<BimElementExternalDto> BimElements { get; set; }
    }
}
