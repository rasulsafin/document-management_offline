namespace MRS.Bim.DocumentManagement.Utilities
{
    public interface ICloudAuth
    {
        AccessProperty AccessProperty { get; set; }
        AppProperty appProperty { get; set; }
        double timeout { get; set; }
    }
}
