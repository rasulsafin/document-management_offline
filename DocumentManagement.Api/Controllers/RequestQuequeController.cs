using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RequestQuequeController : Controller
    {
        private readonly RequestQuequeService service;

        public RequestQuequeController(RequestQuequeService service) => this.service = service;

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetProgress([FromRoute] string id)
        {
            try
            {
                return Ok(service.GetProgress(id));
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
