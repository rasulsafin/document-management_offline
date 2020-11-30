using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class SynchronizedAuthorizationService : AuthorizationService
    {
        private readonly IUserContext userContext;
        private readonly List<string> currentUserRoles = new List<string>();

        public SynchronizedAuthorizationService(DMContext context, IUserContext userContext) : base(context)
        {
            this.userContext = userContext;
        }

        private bool IsCurrentUser(ID<UserDto> id) => id.IsValid && id == userContext.CurrentUser.ID;

        public override async Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            var isAdded = await base.AddRole(userID, role);
            if (isAdded && IsCurrentUser(userID))
                currentUserRoles.Add(role);
            return isAdded;
        }

        public override async Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            var isRemoved = await base.RemoveRole(userID, role);
            if (isRemoved && IsCurrentUser(userID))
                currentUserRoles.Remove(role);
            return isRemoved;
        }

        public override async Task<bool> IsInRole(ID<UserDto> userID, string role)
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
