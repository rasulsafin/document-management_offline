using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ConnectionInfoAuthFieldValuesResolver : IValueResolver<ConnectionInfo, IConnectionInfoDto, IDictionary<string, string>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionInfoAuthFieldValuesResolver> logger;

        public ConnectionInfoAuthFieldValuesResolver(
            CryptographyHelper helper,
            ILogger<ConnectionInfoAuthFieldValuesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionInfoAuthFieldValuesResolver created");
        }

        public IDictionary<string, string> Resolve(ConnectionInfo source, IConnectionInfoDto destination, IDictionary<string, string> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");
            var dictionary = new Dictionary<string, string>();
            foreach (var property in source.AuthFieldValues ?? ArraySegment<AuthFieldValue>.Empty)
                dictionary.Add(property.Key, helper.DecryptAes(property.Value));

            logger.LogDebug("Created dictionary with keys: {@Keys}", dictionary.Keys);
            return dictionary;
        }
    }
}
