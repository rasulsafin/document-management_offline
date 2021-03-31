using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Utility.Factories
{
    public class SynchronizationDataFactory : IFactory<IServiceScope, SynchronizingData>
    {
        private readonly Func<IServiceScope, IMapper> getMapper;
        private readonly Func<IServiceScope, DMContext> getContext;

        public SynchronizationDataFactory(
            Func<IServiceScope, IMapper> getMapper,
            Func<IServiceScope, DMContext> getContext)
        {
            this.getMapper = getMapper;
            this.getContext = getContext;
        }

        public SynchronizingData Create(IServiceScope scope)
            => new SynchronizingData
            {
                Context = getContext(scope),
                Mapper = getMapper(scope),
            };
    }
}
