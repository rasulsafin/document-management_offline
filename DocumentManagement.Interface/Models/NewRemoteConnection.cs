using System.Collections.Generic;

namespace DocumentManagement.Interface.Models
{
    public struct NewRemoteConnection
    {
        public NewRemoteConnection(ID<RemoteConnectionInfo> iD, IReadOnlyDictionary<string, string> authData)
        {
            ID = iD;
            AuthData = authData;
        }

        public ID<RemoteConnectionInfo> ID { get; }
        public IReadOnlyDictionary<string, string> AuthData { get; }
    }
}
