using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Services;
using System.Linq;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly DMContext context;

        public AuthorizationService(DMContext context)
        {
            this.context = context;
        }

        public virtual async Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            var user = await context.Users.FindAsync((int)userID);
            if (user == null)
                throw new ArgumentException($"User with key {userID} not found");
            //return false;
            try
            {
                if (await IsInRole(userID, role))
                    return false;

                var storedRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == role);
                if (storedRole == null)
                {
                    storedRole = new Database.Models.Role() { Name = role };
                    await context.Roles.AddAsync(storedRole);
                    await context.SaveChangesAsync();
                }
                var userRoleLink = new Database.Models.UserRole() { RoleID = storedRole.ID, UserID = user.ID };
                context.UserRoles.Add(userRoleLink);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException($"Can't assign role {role} to user {userID}", ex.InnerException);
            }
        }

        public virtual async Task<IEnumerable<string>> GetAllRoles()
        {
            var dbRoles = await context.Roles.ToListAsync();
            return dbRoles.Select(x => x.Name).ToList();
        }

        public virtual async Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID)
        {
            var id = (int)userID;
            return await context.Users
                .Where(x => x.ID == id)
                .SelectMany(x => x.Roles)
                .Select(x => x.Role.Name)
                .ToListAsync();
        }

        public virtual async Task<bool> IsInRole(ID<UserDto> userID, string role)
        {
            var id = (int)userID;
            return await context.UserRoles
                .Where(x => x.UserID == id)
                .Select(x => x.Role)
                .AnyAsync(x => x.Name == role);
        }

        public virtual async Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            var iuserID = (int)userID;
            var user = await context.Users.FindAsync(iuserID);
            if (user == null)
                return false;

            var links = await context.UserRoles
                .Where(x => x.Role.Name == role)
                .Where(x => x.UserID == iuserID)
                .ToListAsync();
            if (!links.Any())
                return false;
            context.UserRoles.RemoveRange(links);
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
    }
}
