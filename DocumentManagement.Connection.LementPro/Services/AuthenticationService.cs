using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class AuthenticationService : IDisposable
    {
        private HttpRequestUtility requestUtility;

        // Is created as scoped as this service
        private ConnectionInfoExternalDto connectionInfoDto;

        public AuthenticationService(HttpRequestUtility requestUtility)
        {
            this.requestUtility = requestUtility;
            this.requestUtility.AuthenticationService = this;
        }

        /// <summary>
        /// ctor used only to check the connection info.
        /// </summary>
        /// <param name="connectionInfo">Connection info to check.</param>
        internal AuthenticationService(ConnectionInfoExternalDto connectionInfo)
            => connectionInfoDto = connectionInfo;

        internal string AccessToken
        {
            get => GetValueOrDefault(connectionInfoDto.AuthFieldValues, AUTH_NAME_TOKEN);
            set => SetAuthValue(connectionInfoDto, AUTH_NAME_TOKEN, value);
        }

        internal string AccessEnd
        {
            get => GetValueOrDefault(connectionInfoDto.AuthFieldValues, AUTH_NAME_END);
            set => SetAuthValue(connectionInfoDto, AUTH_NAME_END, value);
        }

        public void Dispose()
            => requestUtility.Dispose();

        public async Task<(ConnectionStatusDto authStatus, ConnectionInfoExternalDto updatedInfo)> SignInAsync(ConnectionInfoExternalDto info)
        {
            connectionInfoDto = info;
            var login = connectionInfoDto.AuthFieldValues[AUTH_NAME_LOGIN];
            var password = connectionInfoDto.AuthFieldValues[AUTH_NAME_PASSWORD];

            var (token, expires) = await requestUtility.Connect(login, password);

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expires))
            {
                var errorStatus = new ConnectionStatusDto
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = "Connection with given credentials failed",
                };
                return (errorStatus, null);
            }

            AccessToken = token;
            AccessEnd = expires;
            var successStatus = new ConnectionStatusDto { Status = RemoteConnectionStatus.OK };

            return (successStatus, connectionInfoDto);
        }

        public async Task EnsureAccessValidAsync()
        {
            if (IsAuthorisationAccessValid())
                return;

            await SignInAsync(connectionInfoDto);
        }

        internal bool IsAuthorisationAccessValid()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
                return false;

            if (!DateTime.TryParse(AccessEnd, out var parsedExpiresDate))
                return false;

            if (parsedExpiresDate < DateTime.Now)
                return false;

            return true;
        }

        protected void SetAuthValue(ConnectionInfoExternalDto info, string key, string value)
        {
            info.AuthFieldValues ??= new Dictionary<string, string>();
            SetDictionaryValue(info.AuthFieldValues, key, value);
        }

        protected void SetDictionaryValue(IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }

        protected string GetValueOrDefault(IDictionary<string, string> source, string key)
        {
            if (source != null && source.TryGetValue(key, out var value))
                return value;

            return default;
        }
    }
}
