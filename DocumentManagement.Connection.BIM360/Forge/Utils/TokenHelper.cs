using System;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class TokenHelper : ATokenHelper
    {
        public TokenHelper(ForgeConnection connection)
            : base(connection)
        {
        }

        protected override void SetGettingToken(Func<string> func)
            => connection.GetToken = func;

        protected override string GetToken()
            => connection.GetToken?.Invoke();
    }
}
