using System;

namespace Brio.Docs.Connection.Bim360.Forge.Utils
{
    public class TokenHelper : ATokenHelper
    {
        private readonly ForgeConnection connection;

        public TokenHelper(ForgeConnection connection)
        {
            this.connection = connection;
        }

        protected override void SetGettingToken(Func<string> func)
            => connection.GetToken = func;

        protected override string GetToken()
            => connection.GetToken?.Invoke();
    }
}
