using System.Collections.Generic;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ConnectionTypeAppPropertiesResolver : IValueResolver<ConnectionType, ConnectionTypeDto, IDictionary<string, string>>
    {
        private readonly CryptographyHelper helper;

        public ConnectionTypeAppPropertiesResolver(CryptographyHelper helper)
        {
            this.helper = helper;
        }

        public IDictionary<string, string> Resolve(ConnectionType source, ConnectionTypeDto destination, IDictionary<string, string> destMember, ResolutionContext context)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var property in source.AppProperties)
            {
                dictionary.Add(property.Key, helper.DecryptAes(property.Value));
            }

            return dictionary;
        }
    }
}
