using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IConnectionService
    {
        Task<IEnumerable<RemoteConnectionInfoDto>> GetAvailableConnections();
        Task LinkRemoteConnection(RemoteConnectionToCreateDto connectionInfo);
        Task<ConnectionStatusDto> GetRemoteConnectionStatus();

        Task Reconnect(RemoteConnectionToCreateDto connectionInfo);
        Task<RemoteConnectionInfoDto> GetCurrentConnection();

        Task<IEnumerable<EnumVariantDto>> GetEnumVariants(string dynamicFieldKey);
    }
}
