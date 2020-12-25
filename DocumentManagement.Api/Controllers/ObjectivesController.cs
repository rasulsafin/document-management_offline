using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Threading.Tasks;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ObjectivesController : ControllerBase
    {
        private IObjectiveService service;

        public ObjectivesController(IObjectiveService objectiveService) => service = objectiveService;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ObjectiveToCreateDto data)
        {
            var objectiveToList = await service.Add(data);
            return ValidateFoundObject(objectiveToList);
        }

        [HttpDelete]
        [Route("{objectiveID}")]
        public async Task<IActionResult> Remove([FromRoute] int objectiveID)
        {
            var removed = await service.Remove(new ID<ObjectiveDto>(objectiveID));
            return ValidateFoundRelatedResult(removed);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ObjectiveDto projectData)
        {
            var updated = await service.Update(projectData);
            return ValidateFoundRelatedResult(updated);
        }

        [HttpGet]
        [Route("{objectiveID}")]
        public async Task<IActionResult> Find([FromRoute] int objectiveID)
        {
            var foundObjective = await service.Find(new ID<ObjectiveDto>(objectiveID));
            return ValidateFoundObject(foundObjective);
        }

        [HttpGet]
        [Route("project/{projectID}")]
        public async Task<IActionResult> GetObjectives([FromRoute] int projectID)
        {
            var objectives = await service.GetObjectives(new ID<ProjectDto>(projectID));
            return ValidateCollection(objectives);
        }

        [HttpGet]
        [Route("dynamicfields/{objectiveID}")]
        public async Task<IActionResult> GetRequiredDynamicFields([FromRoute] int objectiveID)
        {
            return Forbid();
            //var dynamicFields = await service.GetRequiredDynamicFields(new ID<ObjectiveDto>(objectiveID));
            //return ValidateCollection(dynamicFields);
        }
    }
}
