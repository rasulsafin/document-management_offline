using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IConnectionService
    {
        Task<IEnumerable<RemoteConnectionInfo>> GetAvailableConnections();
        Task LinkRemoteConnection(RemoteConnectionToCreate connectionInfo);
        Task<ConnectionStatus> GetRemoteConnectionStatus();

        Task Reconnect(RemoteConnectionToCreate connectionInfo);
        Task<RemoteConnectionInfo> GetCurrentConnection();

        Task<IEnumerable<EnumVariant>> GetEnumVariants(string dynamicFieldKey);
    }
}
