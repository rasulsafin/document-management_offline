using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface
{
    /// <summary>
    /// Interface for any type of connection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Connect to remote DM.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <returns>Result success and additional result data.</returns>
        Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info);

        /// <summary>
        /// Current status of the connection.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info);

        /// <summary>
        /// Method fills all the relevant fields in ConnectionInfo
        /// e.g. AuthFieldValues and EnumerationTypes in order to link them to User.
        /// </summary>
        /// <param name="info">ConnectionInfoDto to fill in.</param>
        /// <returns>Filed ConnectionInfoDto.</returns>
        Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info);

        /// <summary>
        /// Get the information about the current Connection.
        /// </summary>
        /// <returns>Filed ConnectionTypeDto.</returns>
        ConnectionTypeExternalDto GetConnectionType();

        /// <summary>
        /// Get the context for working with this connection.
        /// </summary>
        /// <param name="info">ConnectionInfoDto to fill in.</param>
        /// <returns>All data.</returns>
        Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info);
    }
}
