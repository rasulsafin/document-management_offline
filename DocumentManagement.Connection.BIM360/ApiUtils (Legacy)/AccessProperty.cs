using System.Runtime.CompilerServices;

namespace CloudApis.Utils
{
    public class AccessProperty
    {
        private readonly string cloudName;
        private string token;
        private string refreshToken;
        private string end;

        public string Token
        {
            get => GetPrefString(ref token);
            set => SetPrefString(ref token, value);
        }

        public string RefreshToken
        {
            get => GetPrefString(ref refreshToken);
            set => SetPrefString(ref refreshToken, value);
        }

        public string End
        {
            get => GetPrefString(ref end);
            set => SetPrefString(ref end, value);
        }

        public AccessProperty(string cloudName)
        {
            this.cloudName = cloudName;
        }

        private string GetPrefString(ref string set, [CallerMemberName]string method = null)
        {
            return set;
        }

        private void SetPrefString(ref string set, string value, [CallerMemberName] string method = null)
        {
        }
    }
}
