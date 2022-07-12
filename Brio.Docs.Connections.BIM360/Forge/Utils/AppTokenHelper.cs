using System;
using System.IdentityModel.Tokens.Jwt;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    internal class AppTokenHelper : ATokenHelper
    {
        private readonly ForgeConnection connection;

        public AppTokenHelper(ForgeConnection connection)
            => this.connection = connection;

        public bool HasClientID { get; private set; } = false;

        public bool IsNeedReconnect()
        {
            var token = GetToken();
            return string.IsNullOrWhiteSpace(token) ||
                DateTime.UtcNow.Add(ForgeConnection.MIN_TOKEN_LIFE) > new JwtSecurityToken(token).ValidTo;
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