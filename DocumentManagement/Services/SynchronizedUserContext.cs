using System;
using System.Threading.Tasks;
using DocumentManagement.Database;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;

namespace DocumentManagement.Services
{
    internal class SynchronizedUserContext : IUserContext
    {
        private readonly DMContext context;
        private readonly SynchronizedUserService userService;
        private readonly SynchronizedAuthorizationService authorizationService;

        public User CurrentUser => userService.CurrentUser;

        public IUserService UserService => userService;
        public IAuthorizationService AuthorizationService => authorizationService;

        public static async Task<SynchronizedUserContext> TryLogin(DMContext context, string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Login is required");

            var userService = new UserService(context);
            var user = await userService.Find(login);
            if (user == null)
                return null;
            var isPassValid = await userService.VerifyPassword(user.ID, password);
            if (!isPassValid)
                return null;
            var userContext = new SynchronizedUserContext(context, user);
            await userContext.authorizationService.ReloadRoles();
            return userContext;
        }

        public static async Task<SynchronizedUserContext> TryRegister(DMContext context, NewUser userData)
        {
            if (string.IsNullOrEmpty(userData.Login))
                throw new ArgumentException("Login is required");
            if (string.IsNullOrEmpty(userData.Name))
                throw new ArgumentException("Name is required");

            var userService = new UserService(context);
            var id = await userService.Add(userData);
            if (!id.IsValid)
                return null;
            var user = await userService.Find(id);
            if (user == null)
                return null;
            var userContext = new SynchronizedUserContext(context, user);
            await userContext.authorizationService.ReloadRoles();
            return userContext;
        }

        private SynchronizedUserContext(DMContext context, User user)
        {
            this.context = context;
            userService = new SynchronizedUserService(context, user);
            authorizationService = new SynchronizedAuthorizationService(context, this);

            userService.CurrentUserChanged += async (s, e) => await authorizationService.ReloadRoles();
        }

        public bool IsInRole(string role)
        {
            return authorizationService.IsCurrentUserInRole(role);
        }
    }
}
