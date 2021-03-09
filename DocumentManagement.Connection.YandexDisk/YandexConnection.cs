using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexConnection : IConnection
    {
        private const string AUTH_FIELD_KEY_TOKEN = "token";
        private const string NAME_CONNECTION = "Yandex Disk";
        private YandexManager manager;

        public YandexConnection() { }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            try
            {
                if (await IsAuthDataCorrect(info))
                {

                    YandexDiskAuth auth = new YandexDiskAuth();
                    if (!info.AuthFieldValues.ContainsKey(AUTH_FIELD_KEY_TOKEN))
                    {
                        var tokenNew = await auth.GetYandexDiskToken(info);
                        info.AuthFieldValues.Add(AUTH_FIELD_KEY_TOKEN, tokenNew);
                    }

                    var token = info.AuthFieldValues[AUTH_FIELD_KEY_TOKEN];
                    manager = new YandexManager(new YandexDiskController(token));

                    return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.OK, Message = "Good", };
                }

                return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.Error, Message = "Data app not correct", };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatusDto.Error,
                    Message = ex.Message,
                };
            }
        }

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
        {
            return Task.FromResult(info);
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            if (manager != null)
            {
                return await manager.GetStatusAsync();
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatusDto.NeedReconnect,
                Message = "Manager null",
            };
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
        {
            var connect = info.ConnectionType;
            if (connect.Name == NAME_CONNECTION)
            {
                if (connect.AppProperties.ContainsKey(YandexDiskAuth.KEY_CLIENT_ID) &&
                    connect.AppProperties.ContainsKey(YandexDiskAuth.KEY_RETURN_URL))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = NAME_CONNECTION,
                AuthFieldNames = new List<string>() { "token" },
                AppProperties = new Dictionary<string, string>
                {
                    { YandexDiskAuth.KEY_CLIENT_ID, "b1a5acbc911b4b31bc68673169f57051" },
                    { YandexDiskAuth.KEY_CLIENT_SECRET, "b4890ed3aa4e4a4e9e207467cd4a0f2c" },
                    { YandexDiskAuth.KEY_RETURN_URL, @"http://localhost:8000/oauth/" },
                },
            };

            return type;
        }

        public Task<IConnectionContext> GetContext(ConnectionInfoDto info, DateTime lastSynchronizationDate)
        {
            throw new NotImplementedException();
        }
    }
}
