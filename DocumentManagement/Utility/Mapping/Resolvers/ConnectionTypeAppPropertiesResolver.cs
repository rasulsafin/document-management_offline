using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ConnectionTypeAppPropertiesResolver : IValueResolver<ConnectionType, IConnectionTypeDto, IDictionary<string, string>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionTypeAppPropertiesResolver> logger;

        public ConnectionTypeAppPropertiesResolver(CryptographyHelper helper, ILogger<ConnectionTypeAppPropertiesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionTypeAppPropertiesResolver created");
        }

        public IDictionary<string, string> Resolve(ConnectionType source, IConnectionTypeDto destination, IDictionary<string, string> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");
            var dictionary = new Dictionary<string, string>();
            foreach (var property in source.AppProperties)
            {
                dictionary.Add(property.Key, helper.DecryptAes(property.Value));
            }

            logger.LogDebug("Created dictionary with keys: {@Keys}", dictionary.Keys);
            return dictionary;
        }
    }
}
