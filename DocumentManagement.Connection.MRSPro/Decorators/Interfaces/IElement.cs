namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
{
    public interface IElement
    {
        string Id { get; set; }

        string Ancestry { get; set; }

        string Type { get; set; }

        string ParentType { get; set; }

        string ParentId { get; set; }

    }
}
