using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ObjectivesController : ControllerBase
    {
        private IObjectiveService service;

        public ObjectivesController(IObjectiveService objectiveService) => service = objectiveService;

        [HttpGet]
        public async Task<IEnumerable<ObjectiveDto>> GetAllObjectives() => await service.GetAllObjectives();

        [HttpPost]
        public async Task<ID<ObjectiveDto>> Add([FromBody] ObjectiveToCreateDto data) => await service.Add(data);

        [HttpDelete]
        [Route("{objectiveID}")]
        public async Task<bool> Remove([FromRoute] int objectiveID) => await service.Remove(new ID<ObjectiveDto>(objectiveID));

        [HttpPut]
        public async Task Update([FromBody] ObjectiveDto projectData) => await service.Update(projectData);

        [HttpGet]
        [Route("{objectiveID}")]
        public async Task<ObjectiveDto> Find([FromRoute] int objectiveID) => await service.Find(new ID<ObjectiveDto>(objectiveID));

        [HttpGet]
        [Route("project/{projectID}")]
        public async Task<IEnumerable<ObjectiveDto>> GetObjectives([FromRoute] int projectID) => await service.GetObjectives(new ID<ProjectDto>(projectID));

        [HttpGet]
        [Route("dynamicfields")]
        public async Task<IEnumerable<DynamicFieldInfoDto>> GetRequiredDynamicFields() => await service.GetRequiredDynamicFields();
    }
}
