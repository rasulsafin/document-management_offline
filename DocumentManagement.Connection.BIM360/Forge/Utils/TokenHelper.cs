using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class TokenHelper : IDisposable
    {
        private static Dictionary<string, string> tokensContainer;
        private static uint connectionsCount = 0;

        private readonly ForgeConnection connection;

        private string userID = null;

        public TokenHelper(ForgeConnection connection)
        {
            this.connection = connection;
            tokensContainer ??= new Dictionary<string, string>();
            connectionsCount++;
        }

        public void Dispose()
        {
            if (--connectionsCount == 0)
                tokensContainer = null;
            GC.SuppressFinalize(this);
        }

        public void SetToken(string newToken)
        {
            if (!string.IsNullOrWhiteSpace(userID))
            {
                if (tokensContainer.ContainsKey(userID))
                    tokensContainer[userID] = newToken;
                else
                    tokensContainer.Add(userID, newToken);
            }

            connection.GetToken = () => newToken;
        }

        public void SetUserID(string userID)
        {
            if (this.userID == userID)
                return;

            if (!string.IsNullOrWhiteSpace(this.userID))
                throw new Exception("User ID is already set");

            if (string.IsNullOrWhiteSpace(userID))
                return;

            this.userID = userID;

            if (!tokensContainer.ContainsKey(userID))
                tokensContainer.Add(this.userID, connection.GetToken());
            else if (!string.IsNullOrWhiteSpace(connection.GetToken()))
                tokensContainer[userID] = connection.GetToken();
            else
                connection.GetToken = () => tokensContainer[userID];
        }

        public void SetInfo(string newUserID, string newToken)
        {
            SetToken(newToken);
            SetUserID(newUserID);
        }
    }
}
