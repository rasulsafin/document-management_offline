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
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class UserService : IUserService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly CryptographyHelper cryptographyHelper;
        private readonly ILogger<UserService> logger;

        public UserService(
            DMContext context,
            IMapper mapper,
            CryptographyHelper helper,
            ILogger<UserService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            cryptographyHelper = helper;
            this.logger = logger;
            logger.LogTrace("UserService created");
        }

        public virtual async Task<ID<UserDto>> Add(UserToCreateDto user)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with user: {@User}", user);
            try
            {
                var userFromDb = await Find(user.Login);
                if (userFromDb != null)
                    throw new ArgumentException("This login is already being used");

                var newUser = mapper.Map<User>(user);
                logger.LogDebug("Mapped user: {@User}", newUser);
                cryptographyHelper.CreatePasswordHash(user.Password, out byte[] passHash, out byte[] passSalt);
                newUser.PasswordHash = passHash;
                newUser.PasswordSalt = passSalt;
                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();

                var userID = new ID<UserDto>(newUser.ID);
                return userID;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add user {@User}", user);
                throw;
            }
        }

        public virtual async Task<bool> Delete(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Delete started with userID: {@UserID}", userID);

            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);

                context.Users.Remove(user);
                await context.SaveChangesAsync();

                var orphanRoles = await context.Roles
                    .Include(x => x.Users)
                    .Where(x => !x.Users.Any())
                    .ToListAsync();
                logger.LogDebug("Orphan roles: {@OrphanRoles}", orphanRoles);

                if (orphanRoles.Any())
                {
                    context.Roles.RemoveRange(orphanRoles);
                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't delete user {UserID}", userID);
                throw;
            }
        }

        public async Task<bool> Exists(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Exists started with userID: {UserID}", userID);
            try
            {
                return await context.Users.AnyAsync(x => x.ID == (int)userID);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get user {UserID}", userID);
                throw;
            }
        }

        public async Task<bool> Exists(string login)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Exists started with login: {Login}", login);
            try
            {
                login = login?.Trim();
                return await context.Users.AnyAsync(x => x.Login == login);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get user {Login}", login);
                throw;
            }
        }

        public async Task<UserDto> Find(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started userID: {UserID}", userID);
            try
            {
                var dbUser = await context.Users
                    .Include(x => x.ConnectionInfo)
                        .ThenInclude(c => c.ConnectionType)
                    .FirstOrDefaultAsync(x => x.ID == (int)userID);
                logger.LogDebug("Found user: {@User}", dbUser);

                if (dbUser == null)
                    throw new ArgumentNullException($"User with key {userID} was not found");

                return mapper.Map<UserDto>(dbUser);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get user {UserID}", userID);
                throw;
            }
        }

        public async Task<UserDto> Find(string login)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with login: {Login}", login);
            try
            {
                login = login?.Trim();
                var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Login == login);
                logger.LogDebug("Found user: {@User}", dbUser);
                if (dbUser == null)
                    throw new ArgumentNullException($"User with login {login} was not found");

                return mapper.Map<UserDto>(dbUser);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get user {Login}", login);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllUsers started");
            try
            {
                var dbUsers = await context.Users.ToListAsync();
                logger.LogDebug("Found users: {@Users}", dbUsers);
                return dbUsers.Select(x => mapper.Map<UserDto>(x)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of users");
                throw;
            }
        }

        public virtual async Task<bool> Update(UserDto user)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with user: {@User}", user);
            try
            {
                var storedUser = await GetUserChecked(user.ID);
                logger.LogDebug("Found user: {@User}", storedUser);
                storedUser.Login = user.Login;
                storedUser.Name = user.Name;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't update user {@User}", user);
                throw;
            }
        }

        public virtual async Task<bool> UpdatePassword(ID<UserDto> userID, string newPass)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("UpdatePassword started with userID: {@UserID}", userID);
            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);
                cryptographyHelper.CreatePasswordHash(newPass, out byte[] passHash, out byte[] passSalt);
                user.PasswordHash = passHash;
                user.PasswordSalt = passSalt;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't update password of user {UserID}", userID);
                throw;
            }
        }

        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("VerifyPassword started with userID: {@UserID}", userID);
            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);
                var result = cryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);

                if (!result)
                    throw new ArgumentException("Wrong password!");

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't verify password of user {UserID}", userID);
                throw;
            }
        }

        private async Task<User> GetUserChecked(ID<UserDto> userID)
        {
            logger.LogTrace("GetUserChecked started with userID: {UserID}", userID);
            var id = (int)userID;
            var user = await context.Users.FindAsync(id);
            logger.LogDebug("Found user: {@User}", user);
            if (user == null)
                throw new ArgumentNullException($"User with key {userID} was not found");
            return user;
        }
    }
}
