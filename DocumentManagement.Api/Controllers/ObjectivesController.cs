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
        public async Task<ID<ObjectiveDto>> Add(ObjectiveToCreateDto data) => await service.Add(data);

        [HttpDelete]
        public async Task<bool> Remove(ID<ObjectiveDto> objectiveID) => await service.Remove(objectiveID);

        [HttpPut]
        public async Task Update(ObjectiveDto projectData) => await service.Update(projectData);

        [HttpGet]
        [Route("find")]
        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID) => await service.Find(objectiveID);

        [HttpGet]
        [Route("project")]
        public async Task<IEnumerable<ObjectiveDto>> GetObjectives(ID<ProjectDto> projectID) => await service.GetObjectives(projectID);

        [HttpGet]
        [Route("dynamicfields")]
        public async Task<IEnumerable<DynamicFieldInfoDto>> GetRequiredDynamicFields() => await service.GetRequiredDynamicFields();
    }
}
