using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private IConnectionService service;

        public ConnectionsController(IConnectionService connectionService) => service = connectionService;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConnectionInfoToCreateDto connectionInfo)
        {
            var result = await service.Add(connectionInfo);
            return Ok(result);
        }

        [HttpGet]
        [Route("connect/{userID}")]
        public async Task<IActionResult> Connect([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<IActionResult> Get([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> GetRemoteConnectionStatus()
        {
            throw new System.NotImplementedException();
        }

        [HttpPost]
        [Route("startsyncronization/{userID}")]
        public async Task<IActionResult> StartSyncronization([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost]
        [Route("stopsyncronization/{userID}")]
        public async Task<IActionResult> StopSyncronization([FromRoute] int userID)
        {
            throw new System.NotImplementedException();
        }

//        [HttpHead]
//        [Route("syncStart")]
//        public async Task<IActionResult> StartSynchronizeAsync()
//        {
//            bool res = await service.StartSync();

//            // TODO: Отправить пользователью результат
//            return Accepted();
//        }

//        [HttpGet]
//        [Route("progress")]
//        public async Task<IActionResult> GetProgressSyncAsync()
//        {
//            ProgressSync progress = await service.GetProgressSync();
//            return ValidateFoundObject(progress);
//        }

//        [HttpPost]
//        [Route("syncStop")]
//        public IActionResult StopSynchronize()
//        {
//            service.StopSync();
//            return Accepted();
//        }

//        [HttpGet]
//        [Route("yandex-disk")]
//        public async Task AuthenticateToExternalSystem()
//        {
//            // information:  https://yandex.ru/dev/oauth/doc/dg/reference/desktop-client.html
//            var context = this.HttpContext;
//            var response = string.Empty;
//            if (context.Request.Query.ContainsKey("access_token"))
//            {
//                var access_token = context.Request.Query["access_token"];

//                await service.TokenYandexDisk(access_token);
//                await service.StartSync();
//                response = @"<!doctype html>
//<html>
//<head>
//<title>Авторизация</title>
//<script>function onLoad()
//{window.close();}</script>
//</head>
//<body onload=""onLoad()"">You can now close this window!</body></html>";
//            }
//            else
//            {
//                response = @"<!doctype html>
//<html><head>
//<title>Авторизация</title>
//<script>
//function onLoad() 
//{ window.location.href = window.location.href.replace('#', '?')}
//</script></head>
//<body onload=""onLoad()"">...</body></html>";
//            }

//            context.Response.ContentType = "text/html";
//            await HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(response));
//            return;
//        }
    }
}
