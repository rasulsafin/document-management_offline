using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface.Services
{
    public interface IConnectionService
    {
        Task<IEnumerable<RemoteConnectionInfo>> GetAvailableConnections();
        Task LinkRemoteConnection(NewRemoteConnection connectionInfo);
        Task<ConnectionStatus> GetRemoteConnectionStatus();

        Task Reconnect(NewRemoteConnection connectionInfo);
        Task<RemoteConnectionInfo> GetCurrentConnection();

        Task<IEnumerable<EnumVariant>> GetEnumVariants(string dynamicFieldKey);
    }
}
