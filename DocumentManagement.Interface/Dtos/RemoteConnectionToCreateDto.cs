using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct RemoteConnectionToCreateDto
    {
        public RemoteConnectionToCreateDto(ID<RemoteConnectionInfoDto> iD, IReadOnlyDictionary<string, string> authData)
        {
            ID = iD;
            AuthData = authData;
        }

        public ID<RemoteConnectionInfoDto> ID { get; }
        public IReadOnlyDictionary<string, string> AuthData { get; }
    }
}
