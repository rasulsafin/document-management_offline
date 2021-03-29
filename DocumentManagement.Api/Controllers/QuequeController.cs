using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QuequeController : Controller
    {
        private readonly RequestProcessing processing;

        public QuequeController(RequestProcessing processing) => this.processing = processing;

        [HttpGet]
        [Route("{id}")]
        public IActionResult IsComplete([FromRoute] string id)
        {
            return Ok(processing.GetProgress(id));
        }

        [HttpGet]
        [Route("result/{id}")]
        public async Task<IActionResult> GetResult([FromRoute] string id)
        {
            var result = await processing.GetResult(id);
            return Ok(result);
        }
    }
}
