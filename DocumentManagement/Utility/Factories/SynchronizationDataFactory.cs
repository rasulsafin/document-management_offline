using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Utility.Factories
{
    public class SynchronizationDataFactory : IFactory<IServiceScope, SynchronizingData>
    {
        private readonly IFactory<IServiceScope, IMapper> mapperFactory;
        private readonly IFactory<IServiceScope, DMContext> contextFactory;

        public SynchronizationDataFactory(
            IFactory<IServiceScope, IMapper> mapperFactory,
            IFactory<IServiceScope, DMContext> contextFactory)
        {
            this.mapperFactory = mapperFactory;
            this.contextFactory = contextFactory;
        }

        public SynchronizingData Create(IServiceScope scope)
            => new SynchronizingData
            {
                Context = contextFactory.Create(scope),
                Mapper = mapperFactory.Create(scope),
            };
    }
}
