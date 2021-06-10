
namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    public interface IElement
    {
        public string Id { get; set; }

        public string Ancestry { get; set; }

        public long CreatedDate { get; set; }

        public string Type { get; set; }

        public string Owner { get; set; }

        public string ParentType { get; set; }

        public string ParentId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public long LastModifiedDate { get; set; }

        public long DueDate { get; set; }

        public string State { get; set; }
    }
}
