using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ObjectiveToCreateDto
    {
        public ID<UserDto>? AuthorID { get; set; }
        public ID<ProjectDto> ProjectID { get; set; }
        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ObjectiveStatus Status { get; set; }
        public ID<ObjectiveTypeDto> ObjectiveTypeID { get; set; }
        
        public IEnumerable<ItemDto> Items { get; set; }
        public IEnumerable<DynamicFieldToCreateDto> DynamicFields { get; set; }
        public IEnumerable<BimElementDto> BimElements { get; set; }
    }
}
