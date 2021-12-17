using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Integration.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudConnectionMeta : IConnectionMeta
    {
        private const string NAME_CONNECTION = "Brio-Cloud";

        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = NAME_CONNECTION,
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(BrioCloudConnection);

        public Action<IServiceCollection> AddToDependencyInjectionMethod()
           => collection => collection.AddBrioCloud();

        public IEnumerable<GettingPropertyExpression> GetPropertiesForIgnoringByLogging()
            => Enumerable.Empty<GettingPropertyExpression>();
    }
}
