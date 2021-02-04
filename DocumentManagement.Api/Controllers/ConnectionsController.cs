using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private IConnectionService service;

        public ConnectionsController(IConnectionService connectionService) => service = connectionService;

        [HttpGet]
        public async Task<IActionResult> GetAvailableConnections()
        {
            var availableConnections = await service.GetAvailableConnections();
            return ValidateCollection(availableConnections);
        }

        [HttpPost]
        public async Task<IActionResult> LinkRemoteConnection(RemoteConnectionToCreateDto connectionInfo)
        {
            var linked = await service.LinkRemoteConnection(connectionInfo);
            return Forbid();
        }

        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> GetRemoteConnectionStatus()
        {
            var status = await service.GetRemoteConnectionStatus();
            return Forbid();
        }

        [HttpPut]
        public async Task<IActionResult> Reconnect(RemoteConnectionToCreateDto connectionInfo)
        {
            var reconnected = await service.Reconnect(connectionInfo);
            return Forbid();
        }

        [HttpGet]
        [Route("current")]
        public async Task<IActionResult> GetCurrentConnection(int userId)
        {
            var connection = await service.GetCurrentConnection(new ID<UserDto>(userId));
            return ValidateFoundObject(connection);
        }

        [HttpHead]
        [Route("syncStart")]
        public async Task<IActionResult> StartSynchronizeAsync()
        {
            bool res = await service.StartSync();

            // TODO: Отправить пользователью результат
            return Accepted();
        }

        [HttpGet]
        [Route("progress")]
        public async Task<IActionResult> GetProgressSyncAsync()
        {
            ProgressSync progress = await service.GetProgressSync();
            return ValidateFoundObject(progress);
        }

        [HttpPost]
        [Route("syncStop")]
        public IActionResult StopSynchronize()
        {
            service.StopSync();
            return Accepted();
        }

        [HttpGet]
        [Route("yandex-disk")]
        public async Task GetTokenYandexDisk()
        {
            // https://yandex.ru/dev/oauth/doc/dg/reference/desktop-client.html
            var context = this.HttpContext;
            var response = string.Empty;
            if (context.Request.Query.ContainsKey("access_token"))
            {
                var access_token = context.Request.Query["access_token"];

                await service.TokenYandexDisk(access_token);
                await service.StartSync();
                response = @"<!doctype html>
<html>
<head>
<title>Авторизация</title>
<script>function onLoad()
{window.close();}</script>
</head>
<body onload=""onLoad()"">You can now close this window!</body></html>";
            }
            else
            {
                response = @"<!doctype html>
<html><head>
<title>Авторизация</title>
<script>
function onLoad() 
{ window.location.href = window.location.href.replace('#', '?')}
</script></head>
<body onload=""onLoad()"">...</body></html>";
            }

            context.Response.ContentType = "text/html";
            await HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(response));
            return;
        }
    }
}
