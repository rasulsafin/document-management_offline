using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Interface;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;

namespace DocumentManagement
{
    class DocumentManagementApi : IDocumentManagementApi
    {
        public Task<IAuthenticatedAccess> Login(string login, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IAuthenticatedAccess> Register(NewUser data)
        {
            throw new NotImplementedException();
        }
    }

    class AuthenticatedAccess : IAuthenticatedAccess
    {
        public IUserService UserService => throw new NotImplementedException();

        public IProjectService ProjectService => throw new NotImplementedException();

        public IAuthorizationService AuthorizationService => throw new NotImplementedException();

        public IObjectiveService ObjectiveService => throw new NotImplementedException();

        public IItemService ItemService => throw new NotImplementedException();

        public IConnectionService ConnectionService => throw new NotImplementedException();

        public User CurrentUser => throw new NotImplementedException();

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}
