using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldModelToDtoValueResolver : IValueResolver<DynamicField, DynamicFieldDto, object>
    {
        private readonly DMContext dbContext;
        private readonly IMapper mapper;

        public DynamicFieldModelToDtoValueResolver(DMContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public object Resolve(DynamicField source, DynamicFieldDto destination, object destMember, ResolutionContext context)
        {
            var type = (DynamicFieldType)Enum.Parse(typeof(DynamicFieldType), source.Type);

            return type switch
            {
                DynamicFieldType.OBJECT => null,
                DynamicFieldType.STRING => source.Value,
                DynamicFieldType.BOOL => bool.Parse(source.Value),
                DynamicFieldType.INTEGER => int.Parse(source.Value),
                DynamicFieldType.FLOAT => float.Parse(source.Value),
                DynamicFieldType.ENUM => GetEnum(source.Value),
                DynamicFieldType.DATE => DateTime.Parse(source.Value),
                _ => null,
            };
        }

        private Enumeration GetEnum(string valueFromDb)
        {
            var enumValue = dbContext.EnumerationValues
               .Include(x => x.EnumerationType)
                    .ThenInclude(x => x.EnumerationValues)
               .FirstOrDefault(x => x.ID == int.Parse(valueFromDb));

            var type = mapper.Map<EnumerationTypeDto>(enumValue.EnumerationType);
            var value = mapper.Map<EnumerationValueDto>(enumValue);

            return new Enumeration() { EnumerationType = type, Value = value, };
        }
    }
}
