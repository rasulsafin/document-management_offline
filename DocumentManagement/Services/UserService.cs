using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Services
{
    public class UserService : IUserService
    {
        protected readonly DMContext context;
        protected readonly IMapper mapper;
        private readonly ISyncService synchronizator;

        public UserService(DMContext context
            , IMapper mapper
            , ISyncService synchronizator
            )
        {
            this.context = context;
            this.mapper = mapper;
            this.synchronizator = synchronizator;
        }

        private async Task<Database.Models.User> GetUserChecked(ID<UserDto> userID)
        {
            var id = (int)userID;
            var user = await context.Users.FindAsync(id);
            if(user == null)
                throw new ArgumentException($"User with key {userID} not found");            
            return user;
        }

        public virtual async Task<ID<UserDto>> Add(UserToCreateDto data)
        {
            try
            {
                Utility.CryptographyHelper.CreatePasswordHash(data.Password, out byte[] passHash, out byte[] passSalt);
                var user = mapper.Map<User>(data);
                user.PasswordHash = passHash;
                user.PasswordSalt = passSalt;
                //context.Users.
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var userID = new ID<UserDto>(user.ID);
                synchronizator.AddChange(userID);
                return userID;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException("Can't add new user", ex.InnerException);
            }
        }

        public virtual async Task<bool> Delete(ID<UserDto> userID)
        {
            var id = (int)userID;
            var user = await context.Users.FindAsync(id);
            if (user == null)
                return false;

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            var orphanRoles = await context.Roles
                .Include(x => x.Users)
                .Where(x => !x.Users.Any())
                .ToListAsync();
            if (orphanRoles.Any())
            {
                context.Roles.RemoveRange(orphanRoles);
                await context.SaveChangesAsync();
            }
            synchronizator.AddChange(userID);
            return true;
        }

        public async Task<bool> Exists(ID<UserDto> userID)
        {
            return await context.Users.AnyAsync(x => x.ID == (int)userID);
        }

        public async Task<bool> Exists(string login)
        {
            login = login.Trim();
            return await context.Users.AnyAsync(x => x.Login == login);
        }

        public async Task<UserDto> Find(ID<UserDto> userID)
        {
            var dbUser = await context.Users.FindAsync((int)userID);
            return dbUser != null ? mapper.Map<UserDto>(dbUser) : null;
        }

        public async Task<UserDto> Find(string login)
        {
            login = login.Trim();
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Login == login);
            return dbUser != null ? mapper.Map<UserDto>(dbUser) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            var dbUsers = await context.Users.ToListAsync();
            return dbUsers.Select(x => mapper.Map<UserDto>(x)).ToList();
        }

        public virtual async Task<bool> Update(UserDto user)
        {
            try
            {
                var storedUser = await GetUserChecked(user.ID);
                storedUser.Login = user.Login;
                storedUser.Name = user.Name;
                await context.SaveChangesAsync();
                synchronizator.AddChange(user.ID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> UpdatePassword(ID<UserDto> userID, string newPass)
        {
            try
            {
                var user = await GetUserChecked(userID);
                Utility.CryptographyHelper.CreatePasswordHash(newPass, out byte[] passHash, out byte[] passSalt);
                user.PasswordHash = passHash;
                user.PasswordSalt = passSalt;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password)
        {
            var user = await GetUserChecked(userID);
            return Utility.CryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
        }
    }
}
