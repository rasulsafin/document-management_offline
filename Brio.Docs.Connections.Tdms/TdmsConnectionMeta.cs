using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Integration.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Brio.Docs.Connections.Tdms
{
    public class TdmsConnectionMeta : IConnectionMeta
    {
        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "tdms",
                AuthFieldNames = new List<string>
                {
                    Auth.LOGIN,
                    Auth.PASSWORD,
                    Auth.SERVER,
                    Auth.DATABASE,
                },
                AppProperties = new Dictionary<string, string>(),
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(TdmsConnection);

        public Action<IServiceCollection> AddToDependencyInjectionMethod()
            => collection => collection.AddTdms();

        public IEnumerable<GettingPropertyExpression> GetPropertiesForIgnoringByLogging()
            => Enumerable.Empty<GettingPropertyExpression>();
    }
}
