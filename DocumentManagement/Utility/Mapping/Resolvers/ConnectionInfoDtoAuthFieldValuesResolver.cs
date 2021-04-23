using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ConnectionInfoDtoAuthFieldValuesResolver : IValueResolver<IConnectionInfoDto, ConnectionInfo, ICollection<AuthFieldValue>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionInfoDtoAuthFieldValuesResolver> logger;

        public ConnectionInfoDtoAuthFieldValuesResolver(
            CryptographyHelper helper,
            ILogger<ConnectionInfoDtoAuthFieldValuesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionInfoDtoAuthFieldValuesResolver created");
        }

        public ICollection<AuthFieldValue> Resolve(IConnectionInfoDto source, ConnectionInfo destination, ICollection<AuthFieldValue> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");
            var list = new List<AuthFieldValue>();
            foreach (var property in source.AuthFieldValues)
            {
                list.Add(new AuthFieldValue()
                {
                    Key = property.Key,
                    Value = helper.EncryptAes(property.Value),
                });
            }

            logger.LogDebug("Created list: {@List}", list);
            return list;
        }
    }
}
