using System.Collections.Generic;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ConnectionInfoDtoAuthFieldValuesResolver : IValueResolver<IConnectionInfoDto, ConnectionInfo, ICollection<AuthFieldValue>>
    {
        private readonly CryptographyHelper helper;

        public ConnectionInfoDtoAuthFieldValuesResolver(CryptographyHelper helper)
        {
            this.helper = helper;
        }

        public ICollection<AuthFieldValue> Resolve(IConnectionInfoDto source, ConnectionInfo destination, ICollection<AuthFieldValue> destMember, ResolutionContext context)
        {
            var list = new List<AuthFieldValue>();
            foreach (var property in source.AuthFieldValues)
            {
                list.Add(new AuthFieldValue()
                {
                    Key = property.Key,
                    Value = helper.EncryptAes(property.Value),
                });
            }

            return list;
        }
    }
}
