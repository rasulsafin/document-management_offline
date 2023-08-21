using System.ComponentModel.DataAnnotations;
using Brio.Docs.Client.Converters;
using Brio.Docs.Common.Dtos;
using Newtonsoft.Json;

namespace Brio.Docs.Client.Dtos
{
    public class ItemDto
    {
        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ItemDto> ID { get; set; }

        [Required(ErrorMessage = "ValidationError_PathIsRequired")]
        public string RelativePath { get; set; }

        public ItemType ItemType { get; set; }
    }
}
