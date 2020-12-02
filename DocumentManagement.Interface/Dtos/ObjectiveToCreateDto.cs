using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ObjectiveToCreateDto
    {
        public ID<UserDto>? AuthorID { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public ID<ProjectDto> ProjectID { get; set; }
        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }
        
        public string Title { get; set; }
        public string Description { get; set; }
        public ObjectiveStatusDto Status { get; set; }
        public ID<ObjectiveTypeDto> TaskType { get; set; }
        
        public IEnumerable<DynamicFieldToCreateDto> DynamicFields { get; set; }
        public IEnumerable<BimElementDto> BimElements { get; set; }
    }
}
