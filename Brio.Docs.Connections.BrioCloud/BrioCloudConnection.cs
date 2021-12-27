using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.BrioCloud.Synchronization;
using Brio.Docs.External.CloudBase;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudConnection : IConnection
    {
        public static readonly string NAME_CONNECTION = "Brio-Cloud";

        private BrioCloudManager manager;

        public BrioCloudConnection()
        {
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                if (await IsAuthDataCorrect(info))
                {
                    await InitiateManager(info);
                }

                return await GetStatus(info);
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            if (manager != null)
            {
                return await manager.GetStatusAsync();
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.NeedReconnect,
                Message = "Manager null",
            };
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            var objectiveType = "BrioCloudIssue";
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
            };

            if (string.IsNullOrWhiteSpace(info.UserExternalID))
                info.UserExternalID = Guid.NewGuid().ToString();

            return Task.FromResult(info);
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            await InitiateManager(info);
            return BrioCloudConnectionContext.CreateContext(manager);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await InitiateManager(info);
            return new CommonConnectionStorage(manager);
        }

        private static Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            var connect = info.ConnectionType;
            if (connect.Name == NAME_CONNECTION && info.AuthFieldValues.ContainsKey(BrioCloudAuth.USERNAME) && info.AuthFieldValues.ContainsKey(BrioCloudAuth.PASSWORD))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private Task InitiateManager(ConnectionInfoExternalDto info)
        {
            string username = info.AuthFieldValues[BrioCloudAuth.USERNAME];
            string password = info.AuthFieldValues[BrioCloudAuth.PASSWORD];

            manager = new BrioCloudManager(new BrioCloudController(username, password));

            return Task.FromResult(true);
        }
    }
}
