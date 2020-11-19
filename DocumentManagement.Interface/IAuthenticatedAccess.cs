using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;

namespace DocumentManagement.Interface
{
    public interface IAuthenticatedAccess
    {
        IUserService UserService { get; }
        IProjectService ProjectService { get; }
        IAuthorizationService AuthorizationService { get; }
        IObjectiveService ObjectiveService { get; }
        IItemService ItemService { get; }
        IConnectionService ConnectionService { get; }
        IObjectiveTypeService ObjectiveTypeService { get; }

        /// <summary>
        /// Current user data
        /// </summary>
        User CurrentUser { get; }
        /// <summary>
        /// Check if current user is in passed role
        /// </summary>
        /// <param name="role">User role to be checked</param>
        bool IsInRole(string role);
    }
}
