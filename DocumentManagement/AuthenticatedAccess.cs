using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Services;

namespace MRS.DocumentManagement
{
    internal class AuthenticatedAccess : IAuthenticatedAccess
    {
        private readonly DMContext context;
        private readonly SynchronizedUserContext userContext;

        public AuthenticatedAccess(DMContext context, SynchronizedUserContext userContext)
        {
            this.context = context;
            this.userContext = userContext;

            ProjectService = new ProjectService(context);
            ObjectiveService = new ObjectiveService(context);
            ItemService = new ItemService(context);
            ConnectionService = new ConnectionService(context/*, userContext*/);
            ObjectiveTypeService = new ObjectiveTypeService(context);
        }

        public IUserService UserService => userContext.UserService;
        public IAuthorizationService AuthorizationService => userContext.AuthorizationService;
        public IProjectService ProjectService { get; }
        public IObjectiveService ObjectiveService { get; }
        public IItemService ItemService { get; }
        public IConnectionService ConnectionService { get; }
        public IObjectiveTypeService ObjectiveTypeService { get; }

        public UserDto CurrentUser => userContext.CurrentUser;
        public bool IsInRole(string role) => userContext.IsInRole(role);
    }
}
