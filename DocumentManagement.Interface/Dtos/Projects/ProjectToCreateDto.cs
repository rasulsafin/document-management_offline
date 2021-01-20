using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ProjectToCreateDto
    {
        public ID<UserDto> AuthorID { get; set; }
        public string Title { get; set; }
        public IEnumerable<ItemDto> Items { get; set; }
    }
}
