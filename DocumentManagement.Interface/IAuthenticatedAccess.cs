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

        User CurrentUser { get; }
        bool IsInRole(string role);
    }
}
