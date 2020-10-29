using System;
using static MRS.Bim.Tools.PreferenceHandler;

namespace MRS.Bim.DocumentManagement.Utilities
{
    public class AccessProperty
    {
        public string Token
        {
            get => Get(CreateKeyByMethodName(cloudName), ref token);
            set => Set(CreateKeyByMethodName(cloudName), ref token, value);
        }
        
        public string RefreshToken
        {
            get => Get(CreateKeyByMethodName(cloudName), ref refreshToken);
            set => Set(CreateKeyByMethodName(cloudName), ref refreshToken, value);
        }
        
        public DateTime? End
        {
            get => Get(CreateKeyByMethodName(cloudName), ref end);
            set => Set(CreateKeyByMethodName(cloudName), ref end, value);
        }
        
        private readonly string cloudName;
        private string token;
        private string refreshToken;
        private DateTime? end;

        public AccessProperty(string cloudName)
            => this.cloudName = cloudName;
    }
}
