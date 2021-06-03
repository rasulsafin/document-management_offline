using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Factories
{
    public class ContextFactory : IFactory<ConnectionInfoExternalDto, Bim360ConnectionContext>
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IFactory<IServiceScope, Bim360ConnectionContext> contextFactory;
        private readonly IFactory<IServiceScope, ForgeConnection> forgeConnectionFactory;

        public ContextFactory(
            IServiceScopeFactory scopeFactory,
            IFactory<IServiceScope, Bim360ConnectionContext> contextFactory,
            IFactory<IServiceScope, ForgeConnection> forgeConnectionFactory)
        {
            this.scopeFactory = scopeFactory;
            this.contextFactory = contextFactory;
            this.forgeConnectionFactory = forgeConnectionFactory;
        }

        public Bim360ConnectionContext Create(ConnectionInfoExternalDto info)
        {
            var scope = scopeFactory.CreateScope();
            forgeConnectionFactory.Create(scope).Token = info.AuthFieldValues[Constants.TOKEN_AUTH_NAME];
            var context = contextFactory.Create(scope);
            context.Scope = scope;
            return context;
        }
    }
}
