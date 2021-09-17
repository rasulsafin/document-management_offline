using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    /// <summary>
    /// Helper class for updating the token for all connections of the same user.
    /// </summary>
    public abstract class ATokenHelper : IDisposable
    {
        private static readonly object LOCKER = new ();
        private static Dictionary<string, string> tokensContainer;
        private static uint connectionsCount = 0;

        private string userID = null;

        protected ATokenHelper()
        {
            lock (LOCKER)
            {
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
            SetClientID(newUserID);
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
                SetGettingToken(() => newToken);
            }
        }

        /// <summary>
        /// Sets the id of the current user and updates the token for all connections of the same user.
        /// </summary>
        /// <param name="clientID">Setting id of the current user.</param>
        /// <exception cref="ArgumentException">Throws if user id is already set.</exception>
        public virtual void SetClientID(string clientID)
        {
            if (this.userID == clientID)
                return;

            if (!string.IsNullOrWhiteSpace(this.userID))
                throw new ArgumentException("User ID is already set", nameof(clientID));

            if (string.IsNullOrWhiteSpace(clientID))
                return;

            lock (LOCKER)
            {
                this.userID = clientID;
                if (!tokensContainer.ContainsKey(clientID))
                    tokensContainer.Add(this.userID, GetToken());
                else if (!string.IsNullOrWhiteSpace(GetToken()))
                    tokensContainer[clientID] = GetToken();
            }

            SetGettingToken(
                () =>
                {
                    lock (LOCKER)
                        return tokensContainer[clientID];
                });
        }

        protected abstract void SetGettingToken(Func<string> func);

        protected abstract string GetToken();
    }
}
