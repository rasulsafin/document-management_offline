using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Factories
{
    internal class ContextFactory : IFactory<ConnectionInfoExternalDto, IConnectionContext>
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IFactory<IServiceScope, Bim360ConnectionContext> contextFactory;
        private readonly IFactory<IServiceScope, TokenHelper> tokenHelperFactory;

        public ContextFactory(
            IServiceScopeFactory scopeFactory,
            IFactory<IServiceScope, Bim360ConnectionContext> contextFactory,
            IFactory<IServiceScope, TokenHelper> tokenHelperFactory)
        {
            this.scopeFactory = scopeFactory;
            this.contextFactory = contextFactory;
            this.tokenHelperFactory = tokenHelperFactory;
        }

        public IConnectionContext Create(ConnectionInfoExternalDto info)
        {
            var scope = scopeFactory.CreateScope();
            tokenHelperFactory.Create(scope)
               .SetInfo(info.UserExternalID, info.AuthFieldValues[Constants.TOKEN_AUTH_NAME]);
            var context = contextFactory.Create(scope);
            context.Scope = scope;
            return context;
        }
    }
}
