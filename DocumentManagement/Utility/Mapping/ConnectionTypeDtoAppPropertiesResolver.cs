using System.Collections.Generic;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ConnectionTypeDtoAppPropertiesResolver : IValueResolver<IConnectionTypeDto, ConnectionType, IEnumerable<AppProperty>>
    {
        private readonly CryptographyHelper helper;

        public ConnectionTypeDtoAppPropertiesResolver(CryptographyHelper helper)
        {
            this.helper = helper;
        }

        public IEnumerable<AppProperty> Resolve(IConnectionTypeDto source, ConnectionType destination, IEnumerable<AppProperty> destMember, ResolutionContext context)
        {
            var list = new List<AppProperty>();
            foreach (var property in source.AppProperties)
            {
                list.Add(new AppProperty()
                {
                    Key = property.Key,
                    Value = helper.EncryptAes(property.Value),
                });
            }

            return list;
        }
    }
}
