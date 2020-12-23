using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ObjectiveDto
    {
        public ID<ObjectiveDto> ID { get; set; }
        public ID<ProjectDto> ProjectID { get; set; }
        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }
        public UserDto Author { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ObjectiveStatus Status { get; set; }
        public ObjectiveTypeDto ObjectiveType { get; set; }
        public IEnumerable<DynamicFieldDto> DynamicFields { get; set; }
        public IEnumerable<BimElementDto> BimElements { get; set; }
    }
}
