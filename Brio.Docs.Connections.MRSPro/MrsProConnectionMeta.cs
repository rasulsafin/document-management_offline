using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Integration.Utilities;
using Microsoft.Extensions.DependencyInjection;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro
{
    public class MrsProConnectionMeta : IConnectionMeta
    {
        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "MRSPro",
                AuthFieldNames = new List<string>
                {
                    COMPANY_CODE,
                    AUTH_EMAIL,
                    AUTH_PASS,
                },
                AppProperties = new Dictionary<string, string>(),
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(MrsProConnection);

        public Action<IServiceCollection> AddToDependencyInjectionMethod()
            => collection => collection.AddMrsPro();

        public IEnumerable<GettingPropertyExpression> GetPropertiesForIgnoringByLogging()
            => Enumerable.Empty<GettingPropertyExpression>();
    }
}
