using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct RemoteConnectionToCreateDto
    {
        public ID<RemoteConnectionInfoDto> ID { get; }
        public IReadOnlyDictionary<string, string> AuthData { get; }

        public RemoteConnectionToCreateDto(ID<RemoteConnectionInfoDto> iD, IReadOnlyDictionary<string, string> authData)
        {
            ID = iD;
            AuthData = authData;
        }
    }
}
