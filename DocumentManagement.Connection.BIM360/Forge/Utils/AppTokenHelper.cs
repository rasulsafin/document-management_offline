using System;
using System.IdentityModel.Tokens.Jwt;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    internal class AppTokenHelper : ATokenHelper
    {
        public AppTokenHelper(ForgeConnection connection)
            : base(connection)
        {
        }

        public bool HasClientID { get; private set; } = false;

        public bool IsNeedReconnect()
        {
            var token = GetToken();
            return string.IsNullOrWhiteSpace(token) ||
                DateTime.UtcNow.AddMinutes(3) > new JwtSecurityToken(token).ValidTo;
        }

        public override void SetClientID(string clientID)
        {
            HasClientID = true;
            base.SetClientID(clientID);
        }

        protected override void SetGettingToken(Func<string> func)
            => connection.GetAppToken = func;

        protected override string GetToken()
            => connection.GetAppToken?.Invoke();
    }
}
