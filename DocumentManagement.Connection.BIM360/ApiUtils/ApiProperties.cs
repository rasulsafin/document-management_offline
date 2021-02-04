namespace CloudApis.Utils
{
    public class ApiProperties
    {
        public CloudTypes type;
        public AppProperty applicationProperties;
        public double timeout = 0.1;

        private void Start()
        {
            ICloudAuth auth = null;
            switch (type)
            {
                case CloudTypes.Bim360:
                    auth = Forge.AuthenticationService.Instance;
                    break;
                default:
                    return;
            }
            auth.accessProperty = new AccessProperty(type.ToString());
            auth.appPropery = applicationProperties;
            auth.timeout = timeout;
        }

        public enum CloudTypes
        {
            Bim360,
            Yandex,
        }
    }
}