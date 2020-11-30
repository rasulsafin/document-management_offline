using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ObjectiveTypesController : ControllerBase
    {
        private IObjectiveTypeService service;

        public ObjectiveTypesController(IObjectiveTypeService objectiveTypeService) => service = objectiveTypeService;

        [HttpPost]
        public async Task<ID<ObjectiveTypeDto>> Add([FromBody] string typeName) => await service.Add(typeName);

        [HttpGet]
        [Route("{id}")]
        public async Task<ObjectiveTypeDto> Find([FromRoute] int id) => await service.Find(new ID<ObjectiveTypeDto>(id));

        [HttpGet]
        [Route("name")]
        public async Task<ObjectiveTypeDto> Find([FromQuery] string typename) => await service.Find(typename);

        [HttpGet]
        public async Task<IEnumerable<ObjectiveTypeDto>> GetAllObjectiveTypes() => await service.GetAllObjectiveTypes();
    }
}
