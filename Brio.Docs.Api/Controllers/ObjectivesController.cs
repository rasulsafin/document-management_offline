using Brio.Docs.Api.Validators;
using Brio.Docs.Interface.Dtos;
using Brio.Docs.Interface.Exceptions;
using Brio.Docs.Interface.Filters;
using Brio.Docs.Interface.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for managing Objectives.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ObjectivesController : ControllerBase
    {
        private readonly IObjectiveService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ObjectivesController(IObjectiveService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Add new objective.
        /// </summary>
        /// <param name="data">Data for new objective.</param>
        /// <returns>Added objective.</returns>
        /// <response code="201">Returns created objective.</response>
        /// <response code="400">One/multiple of required values is/are empty.</response>
        /// <response code="500">Something went wrong while creating new objective.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveToListDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            ObjectiveToCreateDto data)
        {
            try
            {
                var objectiveToList = await service.Add(data);
                return Created(string.Empty, objectiveToList);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Delete objectives from database by its id.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>True id objective was deleted.</returns>
        /// <response code="200">Objective was deleted successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Objective was not found.</response>
        /// <response code="500">Something went wrong while deleting Objective.</response>
        [HttpDelete]
        [Route("{objectiveID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remove(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int objectiveID)
        {
            try
            {
                await service.Remove(new ID<ObjectiveDto>(objectiveID));
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Update existing objective.
        /// </summary>
        /// <param name="objectiveData">Objective to  update.</param>
        /// <returns>True if updated successfully.</returns>
        /// <response code="200">Objective was updated successfully.</response>
        /// <response code="400">Some of objective's data is null.</response>
        /// <response code="404">Could not find objective to update.</response>
        /// <response code="500">Something went wrong while updating objective.</response>
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Put")]
            ObjectiveDto objectiveData)
        {
            try
            {
                await service.Update(objectiveData);
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Find and return objective by id if exists.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>Found objective.</returns>
        /// <response code="200">Objective found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find objective.</response>
        /// <response code="500">Something went wrong while retrieving the objective.</response>
        [HttpGet]
        [Route("{objectiveID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int objectiveID)
        {
            try
            {
                var foundObjective = await service.Find(new ID<ObjectiveDto>(objectiveID));
                return Ok(foundObjective);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Return list of objectives, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="filter">Parameters for filtration.</param>
        /// <returns>Collection of objectives.</returns>
        /// <response code="200">Collection of objectives linked to project with the pagination info.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find project to retrieve objective list.</response>
        /// <response code="500">Something went wrong while retrieving the objective list.</response>
        [HttpGet]
        [Route("project/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedListDto<ObjectiveToListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectives(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int projectID,
            [FromQuery]
            ObjectiveFilterParameters filter)
        {
            try
            {
                var objectives = await service.GetObjectives(new ID<ProjectDto>(projectID), filter);
                return Ok(objectives);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Generate report about selected objectives.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Objectives/report? path=C:\\Temp % userID=1 % projectName=ProjectName
        ///     [
        ///        {"id": "1"},
        ///        {"id": "2"},
        ///        {"id": "3"}
        ///     ]
        /// </remarks>
        /// <param name="objectives">List of objective id's.</param>
        /// <param name="path">Path to report storage.</param>
        /// <param name="userID">ID of the user, who generates the report.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>Object representing the result of report creation process.</returns>
        /// <response code="201">Returns objective report creation result.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">One of the objectives is missing.</response>
        /// <response code="500">Something went wrong while generating report.</response>
        [HttpPost]
        [Route("report")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveReportCreationResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateReport(
            [FromBody]
            IEnumerable<ID<ObjectiveDto>> objectives,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_PathIsRequired")]
            string path,
            [FromQuery]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int userID,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_ProjectNameIsRequired")]
            string projectName)
        {
            try
            {
                var result = await service.GenerateReport(objectives, path, userID, projectName);
                return Created(string.Empty, result);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["FailedToCreateReport"], ex.Message);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["FailedToCreateReport"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["FailedToCreateReport"], ex.Message);
            }
        }
    }
}
