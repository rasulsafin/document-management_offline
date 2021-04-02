using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MRS.DocumentManagement.Api.Validators;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    /// <summary>
    /// Controller for managing User entities.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public UsersController(IUserService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get list of all existing users.
        /// </summary>
        /// <returns>List of users.</returns>
        /// <response code="200">Returns found list of users.</response>
        /// <response code="500">Something went wrong while retrieving the users.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await service.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Create new user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Users
        ///     {
        ///        "login": "loginValue",
        ///        "password": "passwordValue",
        ///        "name": "nameValue"
        ///     }
        /// </remarks>
        /// <param name="data">User data.</param>
        /// <returns>Id of the created user.</returns>
        /// <response code="201">Returns created user id.</response>
        /// <response code="500">Something went wrong while creating new user.</response>
        /// <response code="400">User with the same login already exists.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [CheckValidUserToCreate]
            UserToCreateDto data)
        {
            try
            {
                var userId = await service.Add(data);
                return Created(string.Empty, userId);
            }
            catch (InvalidDataException ex)
            {
                return CreateProblemResult(this, 400, localizer["CheckValidUserToCreate_AlredyExists"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Delete existing user.
        /// </summary>
        /// <param name="userID">Id of the user to be deleted.</param>
        /// <returns>True if user is deleted.</returns>
        /// <response code="200">User was deleted successfully.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while deleting user.</response>
        [HttpDelete]
        [Route("{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(
            [FromRoute]
            [CheckValidID]
            int userID)
        {
            try
            {
                await service.Delete(new ID<UserDto>(userID));
                return Ok(true);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Update user's values to given data.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Users
        ///     {
        ///        "id": { "id" : 0 },
        ///        "login": "loginValue",
        ///        "name": "nameValue",
        ///        "role": null
        ///     }
        /// </remarks>
        /// <param name="user">UserDto object.</param>
        /// <response code="200">User was updated successfully.</response>
        /// <response code="404">Could not find user to update.</response>
        /// <response code="500">Something went wrong while updating user.</response>
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UserDto user)
        {
            try
            {
                await service.Update(user);
                return Ok(true);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        ///  Verify password.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="password">Password to verify.</param>
        /// <returns>True if password verified.</returns>
        /// <response code="200">Password verified.</response>
        /// <response code="400">Password was not verified.</response>
        /// <response code="404">Could not find user to verify password.</response>
        /// <response code="500">Something went wrong while verifying.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Route("{userID}/password")]
        public async Task<IActionResult> VerifyPassword(
            [FromRoute]
            [CheckValidID]
            int userID,
            [FromBody]
            [Required(ErrorMessage = "ValidationError_PasswordIsRequired")]
            string password)
        {
            try
            {
                 await service.VerifyPassword(new ID<UserDto>(userID), password);
                 return Ok(true);
            }
            catch (InvalidDataException ex)
            {
                return CreateProblemResult(this, 400, localizer["WrongPassword"], ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError"], ex.Message);
            }
        }

        /// <summary>
        ///  Update password.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="newPass">New password.</param>
        /// <returns>True if password updated.</returns>
        /// <response code="200">Password updated.</response>
        /// <response code="404">Could not find user to update password.</response>
        /// <response code="500">Something went wrong while updating.</response>
        [HttpPut]
        [Route("{userID}/password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePassword(
            [FromRoute]
            [CheckValidID]
            int userID,
            [FromBody]
            [Required(ErrorMessage = "ValidationError_PasswordIsRequired")]
            string newPass)
        {
            try
            {
                await service.UpdatePassword(new ID<UserDto>(userID), newPass);
                return Ok(true);
            }
            catch (ArgumentNullException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Get user by their id.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Found user.</returns>
        /// <response code="200">User found.</response>
        /// <response code="404">Could not find user.</response>
        /// <response code="500">Something went wrong while retrieving the user.</response>
        [HttpGet]
        [Route("{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [CheckValidID]
            int userID)
        {
            try
            {
                var foundUser = await service.Find(new ID<UserDto>(userID));
                return Ok(foundUser);
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
        /// Get user by their login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>Found user.</returns>
        /// <response code="200">User found.</response>
        /// <response code="404">Could not find user.</response>
        /// <response code="500">Something went wrong while retrieving the user.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("find")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find([FromQuery] string login)
        {
            try
            {
                var foundUser = await service.Find(login);
                return Ok(foundUser);
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
        /// Check if user exists by id.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>True if user exists, false otherwise.</returns>
        /// <response code="200">User exists.</response>
        /// <response code="500">Something went wrong while checking the user.</response>
        [HttpGet]
        [Route("exists/{userID}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Exists(
            [FromRoute]
            [CheckValidID]
            int userID)
        {
            try
            {
                var exists = await service.Exists(new ID<UserDto>(userID));
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Check if user exists by login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>True if user exists, false otherwise.</returns>
        /// <response code="200">User exists.</response>
        /// <response code="500">Something went wrong while checking the user.</response>
        [HttpGet]
        [Route("exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Exists([FromQuery] string login)
        {
            try
            {
                var exists = await service.Exists(login);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }
    }
}
