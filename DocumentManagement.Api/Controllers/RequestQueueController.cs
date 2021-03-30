using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RequestQueueController : Controller
    {
        private readonly RequestQueueService service;

        public RequestQueueController(RequestQueueService service) => this.service = service;

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProgress([FromRoute] string id)
        {
            try
            {
                var progress = await service.GetProgress(id);
                return Ok(progress);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("result/{id}")]
        public async Task<IActionResult> GetResult([FromRoute] string id)
        {
            try
            {
                var result = await service.GetResult(id);
                return Ok(result);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("cancel/{id}")]
        public IActionResult Cancel([FromRoute] string id)
        {
            try
            {
                service.Cancel(id);
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
    }
}
