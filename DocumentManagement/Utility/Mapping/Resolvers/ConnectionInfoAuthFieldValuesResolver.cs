using System;
using System.Collections.Generic;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ConnectionInfoAuthFieldValuesResolver : IValueResolver<ConnectionInfo, IConnectionInfoDto, IDictionary<string, string>>
    {
        private readonly CryptographyHelper helper;

        public ConnectionInfoAuthFieldValuesResolver(CryptographyHelper helper)
        {
            this.helper = helper;
        }

        public IDictionary<string, string> Resolve(ConnectionInfo source, IConnectionInfoDto destination, IDictionary<string, string> destMember, ResolutionContext context)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var property in source.AuthFieldValues ?? ArraySegment<AuthFieldValue>.Empty)
                dictionary.Add(property.Key, helper.DecryptAes(property.Value));

            return dictionary;
        }
    }
}
