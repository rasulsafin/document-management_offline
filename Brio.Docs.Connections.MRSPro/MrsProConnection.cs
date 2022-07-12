﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.Logging;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro
{
    public class MrsProConnection : IConnection
    {
        private readonly ILogger<MrsProConnection> logger;
        private readonly AuthenticationService authService;
        private readonly Func<MrsProConnectionContext> getContext;
        private readonly MrsProStorage storage;

        public MrsProConnection(
            ILogger<MrsProConnection> logger,
            AuthenticationService authService,
            Func<MrsProConnectionContext> getContext,
            MrsProStorage storage)
        {
            this.logger = logger;
            this.authService = authService;
            this.getContext = getContext;
            this.storage = storage;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                var authStatus = await authService.SignInAsync(
                    info.AuthFieldValues[AUTH_EMAIL],
                    info.AuthFieldValues[AUTH_PASS],
                    info.AuthFieldValues[COMPANY_CODE]);

                return authStatus;
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = ex.Message };
            }
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
            => getContext();

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            return await authService.TryPing();
        }

        public Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            // TODO: Get Storage
            return Task.FromResult<IConnectionStorage>(storage);
        }

        public async Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto
                {
                   ExternalId = ISSUE_TYPE,
                   Name = "Замечание",
                },
                new ObjectiveTypeExternalDto
                {
                   ExternalId = ELEMENT_TYPE,
                   Name = "Элемент проекта",
                },
            };

            info.UserExternalID = Auth.UserId;

            // TODO: Dynamic fields
            return info;
        }
    }
}