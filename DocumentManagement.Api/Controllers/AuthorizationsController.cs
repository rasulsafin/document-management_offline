using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MRS.DocumentManagement.Api.Validators;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationsController : ControllerBase
    {
        private readonly IAuthorizationService service;

        public AuthorizationsController(IAuthorizationService authorizationService) => service = authorizationService;

        [HttpGet]
        [Route("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await service.GetAllRoles();
            return ValidateCollection(roles);
        }

        [HttpPost]
        [Route("user/roles")]
        public async Task<IActionResult> AddRole([FromQuery] int userID, [FromQuery] string role)
        {
            try
            {
                var added = await service.AddRole(new ID<UserDto>(userID), role);
                return Ok(added);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidDataException)
            {
                return BadRequest();
            }
        }

        [HttpDelete]
        [Route("user/roles")]
        public async Task<IActionResult> RemoveRole([FromQuery] int userID, [FromQuery] string role)
        {
            var removed = await service.RemoveRole(new ID<UserDto>(userID), role);
            return ValidateFoundRelatedResult(removed);
        }

        [HttpGet]
        [Route("user/{userID}/roles")]
        public async Task<IActionResult> GetUserRoles([FromRoute] int userID)
        {
            var roles = await service.GetUserRoles(new ID<UserDto>(userID));
            return ValidateCollection(roles);
        }

        [HttpGet]
        [Route("user/roles")]
        public async Task<IActionResult> IsInRole([FromQuery] int userID, [FromQuery] string role)
        {
            var isInRole = await service.IsInRole(new ID<UserDto>(userID), role);
            return Ok(isInRole);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromBody] string password)
        {
            var identityTuple = await GetIdentity(username, password);
            var identity = identityTuple.Item1;
            var user = identityTuple.Item2;

            if (identity == null)
                return NotFound();

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: identity.Claims,
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var result = (encodedJwt, user);

            return Ok(result);
        }

        private async Task<(ClaimsIdentity, ValidatedUserDto)> GetIdentity(string login, string password)
        {
            var validatedUser = await service.Login(login, password);
            if (validatedUser != null && validatedUser.IsValidationSuccessful)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, validatedUser.User.Login),
                };

                if (validatedUser.User.Role != null)
                    claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, validatedUser.User.Role.Name));

                var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

                return (claimsIdentity, validatedUser);
            }

            return (null, validatedUser);
        }
    }
}
