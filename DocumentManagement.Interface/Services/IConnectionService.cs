﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

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

        Task<IEnumerable<ItemDto>> GetItems(IEnumerable<ID<ItemDto>> itemIds);

        Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds);

        /// <summary>
        /// Start the synchronization process
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task<bool> StartSync();

        /// <summary>
        /// Stop the synchronization process with an intermediate save
        /// </summary>
        void StopSync();

        /// <summary>
        /// Get information about the current synchronization process
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task<ProgressSync> GetProgressSync();

        /// <summary>
        /// Set Token
        /// </summary>
        /// <param name="access_token">string token</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task TokenYandexDisk(string access_token);
    }
}
