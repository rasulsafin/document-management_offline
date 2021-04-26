using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MRS.DocumentManagement.Api.Validators;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Exceptions;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    /// <summary>
    /// Controller for managing Objective Types.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ObjectiveTypesController : ControllerBase
    {
        private readonly IObjectiveTypeService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ObjectiveTypesController(IObjectiveTypeService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Add new objective type by its name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /ObjectiveTypes
        ///         "typeNameValue"
        ///
        /// </remarks>
        /// <param name="typeName">Name of the objective type.</param>
        /// <returns>Id of created objective type.</returns>
        /// <response code="201">Objective type was created.</response>
        /// <response code="400">If type name is null.</response>
        /// <response code="500">Something went wrong while creating objective type.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            string typeName)
        {
            try
            {
                var typeId = await service.Add(typeName);
                return Created(string.Empty, typeId);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Get Objective Type by id.
        /// </summary>
        /// <param name="id">Objective Type's id.</param>
        /// <returns>Found type.</returns>
        /// <response code="200">Objective Type found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find Objective Type.</response>
        /// <response code="500">Something went wrong while retrieving the objective type.</response>
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int id)
        {
            try
            {
                var foundType = await service.Find(new ID<ObjectiveTypeDto>(id));
                return Ok(foundType);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveTypeID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get Objective Type by name.
        /// </summary>
        /// <param name="typename">Name of type.</param>
        /// <returns>Found type.</returns>
        /// <response code="200">Objective Type found.</response>
        /// <response code="400">Invalid type name.</response>
        /// <response code="404">Could not find Objective Type.</response>
        /// <response code="500">Something went wrong while retrieving the objective type.</response>
        [HttpGet]
        [Route("name")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_ObjectiveTypeNameIsRequired")]
            string typename)
        {
            try
            {
                var foundType = await service.Find(typename);
                return Ok(foundType);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveTypeID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get list of objective types accessible to specific Connection Type.
        /// </summary>
        /// <param name="connectionTypeId">Connection type id.</param>
        /// <returns>Collection of Objective Types.</returns>
        /// <response code="200">List of Objective Types found. If Connection type id was null or invalid, method will return default list of objective types. </response>
        /// <response code="404">If Connection type id was valid, but could not find its value in database.</response>
        /// <response code="500">Something went wrong while retrieving the objective type.</response>
        [HttpGet]
        [Route("list/{connectionTypeId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectiveTypes(
            [FromRoute]
            int connectionTypeId)
        {
            try
            {
                var allTypes = await service.GetObjectiveTypes(new ID<ConnectionTypeDto>(connectionTypeId));
                return Ok(allTypes);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidConnectionTypeID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Delete objective type by id.
        /// </summary>
        /// <param name="id">Objective Type's id.</param>
        /// <returns>True, if deletion was successful.</returns>
        /// <response code="200">Objective Type was deleted successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Objective Type was not found.</response>
        /// <response code="500">Something went wrong while deleting Objective Type.</response>
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remove(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int id)
        {
            try
            {
                await service.Remove(new ID<ObjectiveTypeDto>(id));
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveTypeID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }
    }
}
