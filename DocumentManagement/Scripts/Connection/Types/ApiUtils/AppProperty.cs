using MRS.Bim;

namespace MRS.Bim.DocumentManagement.Utilities
{
    public class AppProperty
    {
        public string clientId;
        public string clientSecret;
        public string callBackUrl;

        public static AppProperty LoadFromResources(string name)
        {
            var settings = (AppProperty) BimEnvironment.Instance.LoadResource(name, typeof(AppProperty));
            return settings;
        }
    }
}
