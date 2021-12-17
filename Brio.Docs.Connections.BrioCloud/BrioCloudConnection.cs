using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const string NAME_CONNECTION = "Brio-Cloud";

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
                    if (info.AuthFieldValues == null)
                    {
                        info.AuthFieldValues = new Dictionary<string, string>();
                    }

                    if (!info.AuthFieldValues.ContainsKey(BrioCloudAuth.KEY_CLIENT_ID))
                    {
                        info.AuthFieldValues.Add(BrioCloudAuth.KEY_CLIENT_ID, info.ConnectionType.AppProperties[BrioCloudAuth.KEY_CLIENT_ID]);
                    }

                    if (!info.AuthFieldValues.ContainsKey(BrioCloudAuth.KEY_CLIENT_SECRET))
                    {
                        info.AuthFieldValues.Add(BrioCloudAuth.KEY_CLIENT_SECRET, info.ConnectionType.AppProperties[BrioCloudAuth.KEY_CLIENT_SECRET]);
                    }

                    InitiateManager(info);
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
            await InitiateManagerForSynchronization(info);
            return BrioCloudConnectionContext.CreateContext(manager);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await InitiateManagerForSynchronization(info);
            return new CommonConnectionStorage(manager);
        }

        private Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            var connect = info.ConnectionType;
            if (connect.Name == NAME_CONNECTION)
            {
                if (connect.AppProperties.ContainsKey(BrioCloudAuth.KEY_CLIENT_ID) &&
                    connect.AppProperties.ContainsKey(BrioCloudAuth.KEY_CLIENT_SECRET))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        private async Task InitiateManagerForSynchronization(ConnectionInfoExternalDto info)
        {
            if (info.AuthFieldValues == null || !info.AuthFieldValues.ContainsKey(BrioCloudAuth.KEY_CLIENT_ID) || !info.AuthFieldValues.ContainsKey(BrioCloudAuth.KEY_CLIENT_SECRET))
            {
                await Connect(info, default);
                return;
            }

            InitiateManager(info);
        }

        private void InitiateManager(ConnectionInfoExternalDto info)
        {
            string username = info.AuthFieldValues[BrioCloudAuth.KEY_CLIENT_ID];
            string password = info.AuthFieldValues[BrioCloudAuth.KEY_CLIENT_SECRET];

            manager = new BrioCloudManager(new BrioCloudController(username, password));
        }
    }
}
