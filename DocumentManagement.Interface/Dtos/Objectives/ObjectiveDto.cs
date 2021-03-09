using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ObjectiveDto
    {
        public ID<ObjectiveDto> ID { get; set; }

        public ID<ProjectDto> ProjectID { get; set; }

        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }

        public ID<UserDto> AuthorID { get; set; }

        public ID<ObjectiveTypeDto> ObjectiveTypeID { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public ICollection<ItemDto> Items { get; set; }

        [JsonProperty(ItemConverterType = typeof(DynamicFieldDtoConverter))]
        public ICollection<IDynamicFieldDto> DynamicFields { get; set; }

        public ICollection<BimElementDto> BimElements { get; set; }
    }
}
