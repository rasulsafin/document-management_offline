using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        private async Task<User> GetUserChecked(ID<UserDto> userID)
        {
            logger.LogTrace("GetUserChecked started with userID: {UserID}", userID);
            var id = (int)userID;
            var user = await context.Users.FindAsync(id);
            logger.LogDebug("Found user: {@User}", user);
            if (user == null)
                throw new ArgumentException($"User with key {userID} not found");
            return user;
        }

        public virtual async Task<ID<UserDto>> Add(UserToCreateDto data)
        {
            logger.LogTrace("Add started with data: {@Data}", data);
            try
            {
                cryptographyHelper.CreatePasswordHash(data.Password, out byte[] passHash, out byte[] passSalt);
                var user = mapper.Map<User>(data);
                logger.LogDebug("Mapped user: {@User}", user);
                user.PasswordHash = passHash;
                user.PasswordSalt = passSalt;
                // context.Users.
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                var userID = new ID<UserDto>(user.ID);
                return userID;
            }
            catch (DbUpdateException ex)
            {
                logger.LogInformation(ex.InnerException, "Can't add new user");
                throw new InvalidDataException("Can't add new user", ex.InnerException);
            }
        }

        public virtual async Task<bool> Delete(ID<UserDto> userID)
        {
            logger.LogTrace("Delete started with userID: {@UserID}", userID);
            var id = (int)userID;
            var user = await context.Users.FindAsync(id);
            logger.LogDebug("Found user: {@User}", user);
            if (user == null)
                return false;

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

        public async Task<bool> Exists(ID<UserDto> userID)
        {
            logger.LogTrace("Exists started with userID: {UserID}", userID);
            return await context.Users.AnyAsync(x => x.ID == (int)userID);
        }

        public async Task<bool> Exists(string login)
        {
            logger.LogTrace("Exists started with login: {Login}", login);
            login = login.Trim();
            return await context.Users.AnyAsync(x => x.Login == login);
        }

        public async Task<UserDto> Find(ID<UserDto> userID)
        {
            logger.LogTrace("Find started userID: {UserID}", userID);
            var dbUser = await context.Users.FindAsync((int)userID);
            logger.LogDebug("Found user: {@User}", dbUser);
            return dbUser != null ? mapper.Map<UserDto>(dbUser) : null;
        }

        public async Task<UserDto> Find(string login)
        {
            logger.LogTrace("Find started with login: {Login}", login);
            login = login.Trim();
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Login == login);
            logger.LogDebug("Found user: {@User}", dbUser);
            return dbUser != null ? mapper.Map<UserDto>(dbUser) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            logger.LogTrace("GetAllUsers started");
            var dbUsers = await context.Users.ToListAsync();
            logger.LogDebug("Found users: {@Users}", dbUsers);
            return dbUsers.Select(x => mapper.Map<UserDto>(x)).ToList();
        }

        public virtual async Task<bool> Update(UserDto user)
        {
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
                return false;
            }
        }

        public virtual async Task<bool> UpdatePassword(ID<UserDto> userID, string newPass)
        {
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
                return false;
            }
        }

        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password)
        {
            logger.LogTrace("VerifyPassword started with userID: {@UserID}", userID);
            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);
                return cryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't verify password of user {UserID}", userID);
                return false;
            }
        }
    }
}
