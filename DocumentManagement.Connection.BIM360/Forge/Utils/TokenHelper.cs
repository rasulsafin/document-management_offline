using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    /// <summary>
    /// Helper class for updating the token for all connections of the same user.
    /// </summary>
    public class TokenHelper : IDisposable
    {
        private static readonly object LOCKER = new ();
        private static Dictionary<string, string> tokensContainer;
        private static uint connectionsCount = 0;

        private readonly ForgeConnection connection;

        private string userID = null;

        public TokenHelper(ForgeConnection connection)
        {
            lock (LOCKER)
            {
                this.connection = connection;
                tokensContainer ??= new Dictionary<string, string>();
                connectionsCount++;
            }
        }

        public void Dispose()
        {
            lock (LOCKER)
            {
                if (--connectionsCount == 0)
                    tokensContainer = null;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets a new token for all connections of the current user.
        /// </summary>
        /// <param name="newUserID">Setting id of the current user.</param>
        /// <param name="newToken">The new token.</param>
        /// <exception cref="ArgumentException">Throws if user id is already set.</exception>
        public void SetInfo(string newUserID, string newToken)
        {
            SetToken(newToken);
            SetUserID(newUserID);
        }

        /// <summary>
        /// Sets a new token for all connections of the current user. If no user ID is specified, the token will only be stored for the current connection.
        /// </summary>
        /// <param name="newToken">The new token for connections.</param>
        public void SetToken(string newToken)
        {
            if (!string.IsNullOrWhiteSpace(userID))
            {
                lock (LOCKER)
                {
                    if (tokensContainer.ContainsKey(userID))
                        tokensContainer[userID] = newToken;
                    else
                        tokensContainer.Add(userID, newToken);
                }
            }
            else
            {
                connection.GetToken = () => newToken;
            }
        }

        /// <summary>
        /// Sets the id of the current user and updates the token for all connections of the same user.
        /// </summary>
        /// <param name="userID">Setting id of the current user.</param>
        /// <exception cref="ArgumentException">Throws if user id is already set.</exception>
        public void SetUserID(string userID)
        {
            if (this.userID == userID)
                return;

            if (!string.IsNullOrWhiteSpace(this.userID))
                throw new ArgumentException("User ID is already set", nameof(userID));

            if (string.IsNullOrWhiteSpace(userID))
                return;

            lock (LOCKER)
            {
                this.userID = userID;
                if (!tokensContainer.ContainsKey(userID))
                    tokensContainer.Add(this.userID, connection.GetToken?.Invoke());
                else if (!string.IsNullOrWhiteSpace(connection.GetToken?.Invoke()))
                    tokensContainer[userID] = connection.GetToken();
            }

            connection.GetToken = () =>
            {
                lock (LOCKER)
                    return tokensContainer[userID];
            };
        }
    }
}
