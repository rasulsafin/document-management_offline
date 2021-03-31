using System;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Interface;

namespace  MRS.DocumentManagement.Utility.Factories
{
    public class ConnectionFactory : IFactory<Type, IConnection>, IFactory<IServiceScope, Type, IConnection>
    {
        private readonly Func<IServiceScope, Type, IConnection> func;

        public ConnectionFactory(Func<IServiceScope, Type, IConnection> func)
            => this.func = func;

        public IConnection Create(Type type)
            => func(null, type);

        public IConnection Create(IServiceScope scope, Type type)
            => func(scope, type);
    }
}
