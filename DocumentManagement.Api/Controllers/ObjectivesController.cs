using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MRS.DocumentManagement.Api.Validators;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ObjectivesController : ControllerBase
    {
        private readonly IObjectiveService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ObjectivesController(IObjectiveService objectiveService, IStringLocalizer<SharedLocalization> localizer)
        {
            service = objectiveService;
            this.localizer = localizer;
        }

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

        //[HttpGet]
        //[Route("dynamicfields/{objectiveID}")]
        //public async Task<IActionResult> GetRequiredDynamicFields([FromRoute] int objectiveID)
        //{
        //    var dynamicFields = await service.GetRequiredDynamicFields(new ID<ObjectiveDto>(objectiveID));
        //    return Ok(dynamicFields);
        //    //
        //    //return ValidateCollection(dynamicFields);
        //}

        [HttpPost]
        [Route("report")]
        public async Task<IActionResult> GenerateReport(
            [FromBody]
            IEnumerable<ID<ObjectiveDto>> data,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_PathIsRequired")]
            string path,
            [FromQuery]
            [CheckValidID]
            int userID,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_ProjectNameIsRequired")]
            string projectName)
        {
            try
            {
                var result = await service.GenerateReport(data, path, userID, projectName);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["FailedToCreateReport"], ex.Message);
            }
        }
    }
}
