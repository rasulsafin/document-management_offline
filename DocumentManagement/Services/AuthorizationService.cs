using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DocumentManagement.General.Utils.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly CryptographyHelper cryptographyHelper;
        private readonly ILogger<AuthorizationService> logger;

        public AuthorizationService(DMContext context, IMapper mapper, CryptographyHelper helper, ILogger<AuthorizationService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            cryptographyHelper = helper;
            this.logger = logger;
            logger.LogTrace("AuthorizationService created");
        }

        public virtual async Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("AddRole started with userID = {UserID}, role = {Role}", userID, role);
            var user = await context.Users.FindAsync((int)userID);
            if (user == null)
                throw new ArgumentException($"User with key {userID} not found");
            try
            {
                if (await IsInRole(userID, role))
                    return false;

                var storedRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == role);

                logger.LogDebug("Find stored role {@StoredRole}", storedRole);

                if (storedRole == null)
                {
                    storedRole = new Database.Models.Role() { Name = role };
                    await context.Roles.AddAsync(storedRole);
                    await context.SaveChangesAsync();
                }

                var userRoleLink = new Database.Models.UserRole() { RoleID = storedRole.ID, UserID = user.ID };
                logger.LogDebug("Created user <-> role link: {@UserRoleLink}", userRoleLink);
                await context.UserRoles.AddAsync(userRoleLink);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Can't assign role {Role} to user {UserID}", role, userID);
                throw new InvalidDataException($"Can't assign role {role} to user {userID}", ex.InnerException);
            }
        }

        public virtual async Task<IEnumerable<string>> GetAllRoles()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllRoles started");
            return await context.Roles.Select(x => x.Name).ToListAsync();
        }

        public virtual async Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetUserRoles started with userID: {UserID}", userID);
            var id = (int)userID;
            return await context.Users
                .Where(x => x.ID == id)
                .SelectMany(x => x.Roles)
                .Select(x => x.Role.Name)
                .ToListAsync();
        }

        public virtual async Task<bool> IsInRole(ID<UserDto> userID, string role)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("IsInRole started with userID = {UserID}, role = {Role}", userID, role);
            var id = (int)userID;
            return await context.UserRoles
                .Where(x => x.UserID == id)
                .Select(x => x.Role)
                .AnyAsync(x => x.Name == role);
        }

        public virtual async Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("RemoveRole started with userID = {UserID}, role = {Role}", userID, role);
            var iuserID = (int)userID;
            var user = await context.Users.FindAsync(iuserID);
            if (user == null)
                return false;

            var links = await context.UserRoles
                .Where(x => x.Role.Name == role)
                .Where(x => x.UserID == iuserID)
                .ToListAsync();
            logger.LogDebug("Found links: {@Links}", links);
            if (!links.Any())
                return false;
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

        public async Task<ValidatedUserDto> Login(string username, string password)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Login started for {UserName}", username);
            var dbUser = await context.Users
               .FirstOrDefaultAsync(u => string.Equals(u.Login, username, StringComparison.OrdinalIgnoreCase));
            logger.LogDebug("Found user: {@DbUser}", dbUser);
            if (dbUser == null)
                return null;

            if (!cryptographyHelper.VerifyPasswordHash(password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                logger.LogInformation("Password is incorrect");
                return null;
            }

            var dtoUser = mapper.Map<UserDto>(dbUser);

            if (dbUser.Roles != null && dbUser.Roles.Count > 0)
                dtoUser.Role = new RoleDto { Name = dbUser.Roles.First().Role.Name, User = dtoUser };
            logger.LogDebug("User DTO: {@DtoUser}", dtoUser);

            return new ValidatedUserDto { User = dtoUser, IsValidationSuccessful = true };
        }
    }
}
