using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    internal class UserService : IUserService
    {
        protected readonly DMContext context;

        public UserService(DMContext context)
        {
            this.context = context;
        }

        private async Task<Database.Models.User> GetUserChecked(ID<User> userID)
        {
            var id = (int)userID;
            var user = await context.Users.FindAsync(id);
            if(user == null)
                throw new ArgumentException($"User with key {userID} not found");
            return user;
        }

        public virtual async Task<ID<User>> Add(UserToCreate data)
        {
            try
            {
                Utility.CryptographyHelper.CreatePasswordHash(data.Password, out byte[] passHash, out byte[] passSalt);
                var user = new Database.Models.User()
                {
                    Login = data.Login,
                    Name = data.Name,
                    PasswordHash = passHash,
                    PasswordSalt = passSalt
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return new ID<User>(user.ID);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException("Can't add new user", ex.InnerException);
            }
        }

        public virtual async Task<bool> Delete(ID<User> userID)
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

            return true;
        }

        public async Task<bool> Exists(ID<User> userID)
        {
            return await context.Users.AnyAsync(x => x.ID == (int)userID);
        }

        public async Task<bool> Exists(string login)
        {
            login = login.Trim();
            return await context.Users.AnyAsync(x => x.Login == login);
        }

        public async Task<User> Find(ID<User> userID)
        {
            var dbUser = await context.Users.FindAsync((int)userID);
            if (dbUser != null)
            {
                return new User((ID<User>)dbUser.ID, dbUser.Login, dbUser.Name);
            }
            else 
            {
                return null;
            }
        }

        public async Task<User> Find(string login)
        {
            login = login.Trim();
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Login == login);
            if (dbUser != null)
            {
                return new User((ID<User>)dbUser.ID, dbUser.Login, dbUser.Name);
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            var dbUsers = await context.Users
                .Select(x => new { x.ID, x.Login, x.Name })
                .ToListAsync();
            return dbUsers.Select(x => new User((ID<User>)x.ID, x.Login, x.Name))
                .ToList();
        }

        public virtual async Task Update(User user)
        {
            var storedUser = await GetUserChecked(user.ID);
            storedUser.Login = user.Login;
            storedUser.Name = user.Name;
            await context.SaveChangesAsync();
        }

        public virtual async Task UpdatePassword(ID<User> userID, string newPass)
        {
            var user = await GetUserChecked(userID);
            Utility.CryptographyHelper.CreatePasswordHash(newPass, out byte[] passHash, out byte[] passSalt);
            user.PasswordHash = passHash;
            user.PasswordSalt = passSalt;
            await context.SaveChangesAsync();
        }

        public async Task<bool> VerifyPassword(ID<User> userID, string password)
        {
            var user = await GetUserChecked(userID);
            return Utility.CryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
        }
    }
}
