using static MRS.Bim.Tools.PreferenceHandler;

namespace MRS.Bim.DocumentManagement.Utilities
{
    public class AccessProperty
    {
        public string Token
        {
            get => Get(CreateKeyByMethodName(cloudName),  ref token);
            set => Set(CreateKeyByMethodName(cloudName), out token, value);
        }
        
        public string RefreshToken
        {
            get => Get(CreateKeyByMethodName(cloudName), ref refreshToken);
            set => Set(CreateKeyByMethodName(cloudName), out refreshToken, value);
        }
        
        public string End
        {
            get => Get(CreateKeyByMethodName(cloudName),  ref end);
            set => Set(CreateKeyByMethodName(cloudName), out end, value);
        }
        
        private readonly string cloudName;
        private string token;
        private string refreshToken;
        private string end;

        public AccessProperty(string cloudName)
            => this.cloudName = cloudName;
    }
}
