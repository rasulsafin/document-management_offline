using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Database;
using DocumentManagement.Interface;
using DocumentManagement.Interface.Models;
using DocumentManagement.Services;

[assembly: InternalsVisibleTo("DocumentManagement.Tests")]
namespace DocumentManagement
{
    public class DocumentManagementApi : IDocumentManagementApi
    {
        private readonly DMContext context;

        public DocumentManagementApi(DMContext context)
        {
            this.context = context;
        }
        
        public async Task<IAuthenticatedAccess> Login(string login, string password)
        {
            var userContext = await SynchronizedUserContext.TryLogin(context, login, password);
            if (userContext == null)
                return null;
            return new AuthenticatedAccess(context, userContext);
        }

        public async Task<IAuthenticatedAccess> Register(NewUser data)
        {
            var userContext = await SynchronizedUserContext.TryRegister(context, data);
            if (userContext == null)
                return null;
            return new AuthenticatedAccess(context, userContext);
        }
    }
}
