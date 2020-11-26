using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Services
{
    internal class SynchronizedAuthorizationService : AuthorizationService
    {
        private readonly IUserContext userContext;
        private readonly List<string> currentUserRoles = new List<string>();

        public SynchronizedAuthorizationService(DMContext context, IUserContext userContext) : base(context)
        {
            this.userContext = userContext;
        }

        private bool IsCurrentUser(ID<User> id) => id.IsValid && id == userContext.CurrentUser.ID;

        public override async Task<bool> AddRole(ID<User> userID, string role)
        {
            var isAdded = await base.AddRole(userID, role);
            if (isAdded && IsCurrentUser(userID))
                currentUserRoles.Add(role);
            return isAdded;
        }

        public override async Task<bool> RemoveRole(ID<User> userID, string role)
        {
            var isRemoved = await base.RemoveRole(userID, role);
            if (isRemoved && IsCurrentUser(userID))
                currentUserRoles.Remove(role);
            return isRemoved;
        }

        public override async Task<bool> IsInRole(ID<User> userID, string role)
        {
            if (IsCurrentUser(userID))
            {
                return IsCurrentUserInRole(role);
            }
            return await base.IsInRole(userID, role);
        }

        public bool IsCurrentUserInRole(string role) =>
            currentUserRoles.Contains(role);

        public async Task ReloadRoles()
        {
            if (!userContext.CurrentUser.ID.IsValid)
            {
                currentUserRoles.Clear();
            }
            else 
            {
                var roles = await base.GetUserRoles(userContext.CurrentUser.ID);
                currentUserRoles.Clear();
                currentUserRoles.AddRange(roles);
            }           
        }
    }
}
