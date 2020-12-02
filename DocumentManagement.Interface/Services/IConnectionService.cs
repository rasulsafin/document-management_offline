using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IConnectionService
    {
        Task<IEnumerable<RemoteConnectionInfoDto>> GetAvailableConnections();
        Task<bool> LinkRemoteConnection(RemoteConnectionToCreateDto connectionInfo);
        Task<ConnectionStatusDto> GetRemoteConnectionStatus();

        Task<bool> Reconnect(RemoteConnectionToCreateDto connectionInfo);
        Task<RemoteConnectionInfoDto> GetCurrentConnection(ID<UserDto> userId);

        Task<IEnumerable<EnumVariantDto>> GetEnumVariants(string dynamicFieldKey);
    }
}
