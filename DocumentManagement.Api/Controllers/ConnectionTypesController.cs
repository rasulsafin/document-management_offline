using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator; 

namespace DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionTypesController : ControllerBase
    {
        private IConnectionTypeService service;

        public ConnectionTypesController(IConnectionTypeService connectionTypeService) => service = connectionTypeService;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] string typeName)
        {
            try
            {
                var typeId = await service.Add(typeName);
                return ValidateId(typeId);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Find([FromRoute] int id)
        {
            var foundType = await service.Find(new ID<ConnectionTypeDto>(id));
            return ValidateFoundObject(foundType);
        }

        [HttpGet]
        [Route("name")]
        public async Task<IActionResult> Find([FromQuery] string typename)
        {
            var foundType = await service.Find(typename);
            return ValidateFoundObject(foundType);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConnectionTypes()
        {
            var allTypes = await service.GetAllConnectionTypes();
            return ValidateCollection(allTypes);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Remove([FromRoute] int id)
        {
            try
            {
                var removed = await service.Remove(new ID<ConnectionTypeDto>(id));
                return ValidateFoundRelatedResult(removed);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Route("register")]
        public async Task<IActionResult> Register()
        {
            var result = await service.RegisterAll();
            return result ? (IActionResult)Ok() : BadRequest();
        }
    }
}
