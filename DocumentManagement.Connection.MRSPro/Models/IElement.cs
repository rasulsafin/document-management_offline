namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    public interface IElement
    {
        string Id { get; set; }

        string Ancestry { get; set; }

        long CreatedDate { get; set; }

        string Type { get; set; }

        string Owner { get; set; }

        string ParentType { get; set; }

        string ParentId { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        long LastModifiedDate { get; set; }

        long DueDate { get; set; }

        string State { get; set; }
    }
}
