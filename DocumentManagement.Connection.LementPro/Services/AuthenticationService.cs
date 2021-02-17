using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper.Internal;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class AuthenticationService : IDisposable
    {
        private HttpRequestUtility requestUtility;

        // Is created as scoped as this service
        private ConnectionInfoDto connectionInfoDto;

        public AuthenticationService(HttpRequestUtility requestUtility)
            => this.requestUtility = requestUtility;

        internal string AccessToken
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(AUTH_NAME_TOKEN);
            set => SetAuthValue(connectionInfoDto, AUTH_NAME_TOKEN, value);
        }

        internal string AccessEnd
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(AUTH_NAME_END);
            set => SetAuthValue(connectionInfoDto, AUTH_NAME_END, value);
        }

        public void Dispose()
            => requestUtility.Dispose();

        public async Task<(ConnectionStatusDto authStatus, ConnectionInfoDto updatedInfo)> SignInAsync(ConnectionInfoDto info)
        {
            connectionInfoDto = info;
            var login = connectionInfoDto.AuthFieldValues[AUTH_NAME_LOGIN];
            var password = connectionInfoDto.AuthFieldValues[AUTH_NAME_PASSWORD];

            var (token, expires) = await requestUtility.Connect(login, password);

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expires))
            {
                var errorStatus = new ConnectionStatusDto
                {
                    Status = RemoteConnectionStatusDto.Error,
                    Message = "Connection with given credentials failed",
                };
                return (errorStatus, null);
            }

            AccessToken = token;
            AccessEnd = expires;
            var successStatus = new ConnectionStatusDto { Status = RemoteConnectionStatusDto.OK };

            return (successStatus, connectionInfoDto);
        }

        public async Task<string> EnsureAccessValidAsync()
        {
            if (IsAuthorisationAccessValid())
                return AccessToken;

            await SignInAsync(connectionInfoDto);
            return AccessToken;
        }

        protected bool IsAuthorisationAccessValid()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
                return false;

            if (!DateTime.TryParse(AccessEnd, out var parsedExpiresDate))
                return false;

            if (parsedExpiresDate < DateTime.Now)
                return false;

            return true;
        }

        // TODO Move common with BIM360 methods in common project

        protected void SetAuthValue(ConnectionInfoDto info, string key, string value)
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
    }
}
