using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Interface.Dtos
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
