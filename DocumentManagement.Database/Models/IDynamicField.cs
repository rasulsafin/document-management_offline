namespace MRS.DocumentManagement.Database.Models
{
    public interface IDynamicField
    {
        string Type { get; set; }

        string Value { get; set; }

        int? ConnectionInfoID { get; set; }
    }
}
