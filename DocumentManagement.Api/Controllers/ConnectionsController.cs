using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MRS.DocumentManagement.Api.Validators;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    /// <summary>
    /// Controller for managing RemoteConnections (e.g. YandexDisk, TDMS, BIM360).
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private readonly IConnectionService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ConnectionsController(IConnectionService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Add new ConnectionInfo and link it to User.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///         POST /Connections
        ///         {
        ///             "connectionTypeID": { "id" : 16},
        ///             "userID": { "id" : 101},
        ///             "authFieldValues": {
        ///                 "database": "kosmos",
        ///                 "server": "192.168.100.6\\sqlkosmos",
        ///                 "password": "123",
        ///                 "login": "gureva"
        ///                 }
        ///         }
        /// </remarks>
        /// <param name="connectionInfo">ConnectionInfo to create.</param>
        /// <returns>True if ConnectionInfo was successfully created.</returns>
        /// <response code="201">ConnectionInfo was created.</response>
        /// <response code="400">If something in ConnectionInfo is null.</response>
        /// <response code="404">Could not find user to link connection info to.</response>
        /// <response code="500">Something went wrong while creating ConnectionInfo.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveTypeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            ConnectionInfoToCreateDto connectionInfo)
        {
            try
            {
                var connectionInfoId = await service.Add(connectionInfo);
                return Created(string.Empty, connectionInfoId);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Connect user to Remote connection(e.g. YandexDisk, TDMS, BIM360), using user's ConnectionInfo.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        /// <response code="202">Request is accepted but can take a long time to proceed. Check with the /RequestQueue to get the result.</response>
        /// <response code="500">Something went wrong while server tried to establish connection.</response>
        [HttpGet]
        [Route("connect/{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RequestID), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Connect(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var result = await service.Connect(new ID<UserDto>(userID));
                return Accepted(result);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["CannotConnect"], ex.Message);
            }
        }

        /// <summary>
        /// Get ConnectionInfo for the specific user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Found ConnectionInfo.</returns>
        /// <response code="200">ConnectionInfo found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find User or ConnectionInfo.</response>
        /// <response code="500">Something went wrong while retrieving the ConnectionInfo.</response>
        [HttpGet]
        [Route("{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ConnectionInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var connectionInfoDto = await service.Get(new ID<UserDto>(userID));
                return Ok(connectionInfoDto);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get current status of user's connection.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Status of the connection.</returns>
        /// <response code="200">Status returned.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">Could not find User OR Connection info to get status info.</response>
        /// <response code="500">Something went wrong while trying to get Connection Status.</response>
        [HttpGet]
        [Route("status/{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ConnectionStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRemoteConnectionStatus(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var status = await service.GetRemoteConnectionStatus(new ID<UserDto>(userID));
                return Ok(status);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["SomethingIsMissing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get available to user enumeration values of enum type.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="enumerationTypeID">Enumeration Type's ID.</param>
        /// <returns>Collection of enumeration values.</returns>
        /// <response code="200">Collection of enumeration values returned.</response>
        /// <response code="400">Invalid user id or enumeration type id.</response>
        /// <response code="404">Could not find User OR Enumeration type.</response>
        /// <response code="500">Something went wrong while trying to get EnumerationVariants.</response>
        [HttpGet]
        [Route("enumerationValues")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnumerationVariants(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int enumerationTypeID)
        {
            try
            {
                var result = await service.GetEnumerationVariants(new ID<UserDto>(userID), new ID<EnumerationTypeDto>(enumerationTypeID));
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["SomethingIsMissing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Synchronize user's data.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        /// <response code="202">Request is accepted but can take a long time to proceed. Check with the /RequestQueue to get the result.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while server tried to synchronize.</response>
        [HttpGet]
        [Route("synchronization/{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RequestID), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Synchronize(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var result = await service.Synchronize(new ID<UserDto>(userID));
                return Accepted(result);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["CouldnotSynchronize"], ex.Message);
            }
        }
    }
}
