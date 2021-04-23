using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service to manage RemoteConnections (e.g. YandexDisk, TDMS, BIM360).
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Adds new ConnectionInfo and links it to User.
        /// </summary>
        /// <param name="connectionInfo">ConnectionInfo to create.</param>
        /// <returns>True if ConnectionInfo was successfully created.</returns>
        Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto connectionInfo);

        /// <summary>
        /// Gets ConnectionInfo for the specific user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>ConnectionInfoDto.</returns>
        Task<ConnectionInfoDto> Get(ID<UserDto> userID);

        /// <summary>
        /// Connects user to Remote connection(e.g. YandexDisk, TDMS, BIM360), using user's ConnectionInfo.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        Task<RequestID> Connect(ID<UserDto> userID);

        /// <summary>
        /// Gets current status of user's connection.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Status of the connection.</returns>
        Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID);

        /// <summary>
        /// Gets available to user enumeration values of enum type.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="enumerationTypeID">Enumeration Type's ID.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID);

        /// <summary>
        /// Synchronize user's data.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        Task<RequestID> Synchronize(ID<UserDto> userID);
    }
}
