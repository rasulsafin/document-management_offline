using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Internal;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class AuthenticationService : IDisposable
    {
        private NetConnector connector;

        // Is created as scoped as this service
        private ConnectionInfoDto connectionInfoDto;

        public AuthenticationService(NetConnector connector)
            => this.connector = connector;

        private string AccessToken
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(AUTH_NAME_TOKEN);
            set => SetAuthValue(connectionInfoDto, AUTH_NAME_TOKEN, value);
        }

        private string AccessEnd
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(AUTH_NAME_END);
            set => SetAuthValue(connectionInfoDto, AUTH_NAME_END, value);
        }

        public void Dispose()
            => connector.Dispose();

        public async Task<(ConnectionStatusDto authStatus, ConnectionInfoDto updatedInfo)> SignInAsync(ConnectionInfoDto info)
        {
            connectionInfoDto = info;
            var login = connectionInfoDto.AuthFieldValues[AUTH_NAME_LOGIN];
            var password = connectionInfoDto.AuthFieldValues[AUTH_NAME_PASSWORD];
            var request = connector.CreateRequest(HttpMethod.Post, $"{Resources.UrlServer}{Resources.AuthenticationLoginMethod}");

            var authData = new AuthorizationData
            {
                LoginName = login,
                Password = password,
                RememberMe = true,
            };

            request.Content = new StringContent(authData.ToJson(), Encoding.UTF8, STANDARD_CONTENT_TYPE);
            request.Headers.Add(CONTENT_ACCEPT_LANGUAGE, STANDARD_ACCEPT_LANGUAGE);

            var response = await connector.SendRequestAsync(request);
            var token = ParseCookieFromResponse(response, RESPONSE_COOKIES_AUTH_NAME);
            var expires = ParseCookieFromResponse(response, RESPONSE_COOKIES_EXPIRES_NAME);

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

        private static string ParseCookieFromResponse(HttpResponseMessage response, string cookieName)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookie))
            {
                var cookies = setCookie.FirstOrDefault()?.Split(RESPONSE_COOKIE_VALUES_SEPARATOR);
                if (cookies == null)
                    return default;

                foreach (var cookie in cookies)
                {
                    var parsedValue = cookie.Trim().Split(RESPONSE_COOKIE_KEY_VALUE_SEPARATOR);
                    if (parsedValue.Length == 2 && parsedValue[0] == cookieName)
                    {
                        return parsedValue[1];
                    }
                }
            }

            return default;
        }

        // TODO Move common with BIM360 methods in common project

        private void SetAuthValue(ConnectionInfoDto info, string key, string value)
        {
            info.AuthFieldValues ??= new Dictionary<string, string>();
            SetDictionaryValue(info.AuthFieldValues, key, value);
        }

        private void SetDictionaryValue(IDictionary<string, string> dictionary, string key, string value)
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
