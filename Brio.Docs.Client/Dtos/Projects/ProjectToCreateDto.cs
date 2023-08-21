using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Brio.Docs.Client.Converters;
using Newtonsoft.Json;

namespace Brio.Docs.Client.Dtos
{
    public struct ProjectToCreateDto
    {
        public ID<UserDto> AuthorID { get; set; }

        [Required(ErrorMessage = "ValidationError_ProjectNameIsRequired")]
        public string Title { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }
    }
}
