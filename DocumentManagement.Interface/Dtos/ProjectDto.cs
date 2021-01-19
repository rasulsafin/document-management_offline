using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ProjectDto
    {
        public ID<ProjectDto> ID { get; set; }

        public string Title { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }
    }
}
