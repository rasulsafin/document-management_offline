using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Extensions;

namespace MRS.DocumentManagement.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly CryptographyHelper cryptographyHelper;
        private readonly ILogger<AuthorizationService> logger;

        public AuthorizationService(
            DMContext context,
            IMapper mapper,
            CryptographyHelper helper,
            ILogger<AuthorizationService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            cryptographyHelper = helper;
            this.logger = logger;
            logger.LogTrace("AuthorizationService created");
        }

        public async Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("AddRole started with userID = {UserID}, role = {Role}", userID, role);

            try
            {
                var id = await CheckUser(userID);

                if (await IsInRole(userID, role))
                    throw new ArgumentException($"User with key {userID} already has role {role}");

                var storedRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == role);

                logger.LogDebug("Find stored role {@StoredRole}", storedRole);

                if (storedRole == null)
                {
                    storedRole = new Database.Models.Role() { Name = role };
                    await context.Roles.AddAsync(storedRole);
                    await context.SaveChangesAsync();
                }

                var userRoleLink = new Database.Models.UserRole() { RoleID = storedRole.ID, UserID = id };
                logger.LogDebug("Created user <-> role link: {@UserRoleLink}", userRoleLink);
                await context.UserRoles.AddAsync(userRoleLink);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't assign role {Role} to user {UserID}", role, userID);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetAllRoles()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllRoles started");

            try
            {
                return await context.Roles.Select(x => x.Name).ToListAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of roles");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetUserRoles started with userID: {UserID}", userID);

            try
            {
                var id = await CheckUser(userID);

                return await context.Users
                    .Where(x => x.ID == id)
                    .SelectMany(x => x.Roles)
                    .Select(x => x.Role.Name)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of user's {UserID} roles", userID);
                throw;
            }
        }

        public async Task<bool> IsInRole(ID<UserDto> userID, string role)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("IsInRole started with userID = {UserID}, role = {Role}", userID, role);

            try
            {
                var id = await CheckUser(userID);

                return await context.UserRoles
                    .Where(x => x.UserID == id)
                    .Select(x => x.Role)
                    .AnyAsync(x => x.Name == role);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get result of user {UserID} is in role {Role} or not", userID, role);
                throw;
            }
        }

        public async Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("RemoveRole started with userID = {UserID}, role = {Role}", userID, role);

            try
            {
                var id = await CheckUser(userID);

                var links = await context.UserRoles
                    .Where(x => x.Role.Name == role)
                    .Where(x => x.UserID == id)
                    .ToListAsync();
                logger.LogDebug("Found links: {@Links}", links);
                if (!links.Any())
                    throw new ArgumentException($"User with key {userID} do not have role {role}");

                context.UserRoles.RemoveRange(links);
                await context.SaveChangesAsync();

                var orphanRoles = await context.Roles
                    .Include(x => x.Users)
                    .Where(x => !x.Users.Any())
                    .ToListAsync();
                logger.LogDebug("Found orphanRoles: {@OrphanRoles}", orphanRoles);

                if (orphanRoles.Any())
                {
                    context.Roles.RemoveRange(orphanRoles);
                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove role {Role} from user {UserID}", role, userID);
                throw;
            }
        }

        public async Task<ValidatedUserDto> Login(string username, string password)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Login started for {UserName}", username);

            try
            {
                var dbUser = await context.Users.Include(x => x.ConnectionInfo)
                   .ThenInclude(x => x.ConnectionType)
                   .FindWithIgnoreCaseOrThrowAsync(nameof(User.Login), username);
                logger.LogDebug("Found user: {@DbUser}", dbUser);

                if (!cryptographyHelper.VerifyPasswordHash(password, dbUser.PasswordHash, dbUser.PasswordSalt))
                    throw new ArgumentException($"Wrong password!");

                var dtoUser = mapper.Map<UserDto>(dbUser);

                if (dbUser.Roles != null && dbUser.Roles.Count > 0)
                    dtoUser.Role = new RoleDto { Name = dbUser.Roles.First().Role.Name, User = dtoUser };
                logger.LogDebug("User DTO: {@DtoUser}", dtoUser);

                return new ValidatedUserDto { User = dtoUser, IsValidationSuccessful = true };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't login with username {Username}", username);
                throw;
            }
        }

        private async Task<int> CheckUser(ID<UserDto> userID)
            => (await context.Users.FindOrThrowAsync((int)userID)).ID;
    }
}
