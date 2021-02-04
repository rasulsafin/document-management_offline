namespace CloudApis.Utils
{
    public interface ICloudAuth
    {
        AccessProperty accessProperty { get; set; }
        AppProperty appPropery { get; set; }
        double timeout { get; set; }
    }
}
