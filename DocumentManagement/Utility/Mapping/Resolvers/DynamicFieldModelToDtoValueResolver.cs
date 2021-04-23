using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class DynamicFieldModelToDtoValueResolver : IValueResolver<IDynamicField, DynamicFieldDto, object>
    {
        private readonly DMContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<DynamicFieldModelToDtoValueResolver> logger;

        public DynamicFieldModelToDtoValueResolver(DMContext dbContext, IMapper mapper, ILogger<DynamicFieldModelToDtoValueResolver> logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("DynamicFieldModelToDtoValueResolver created");
        }

        public object Resolve(IDynamicField source, DynamicFieldDto destination, object destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
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
            logger.LogTrace("GetEnum started with valueFromDb: {@ValueFromDb}", valueFromDb);
            var enumValue = dbContext.EnumerationValues
               .Include(x => x.EnumerationType)
                    .ThenInclude(x => x.EnumerationValues)
               .FirstOrDefault(x => x.ID == int.Parse(valueFromDb));

            logger.LogDebug("Found enum value: {@EnumValue}", enumValue);

            var type = mapper.Map<EnumerationTypeDto>(enumValue.EnumerationType);
            logger.LogDebug("Mapped type: {@Type}", type);
            var value = mapper.Map<EnumerationValueDto>(enumValue);
            logger.LogDebug("Mapped value: {@Value}", value);

            return new Enumeration() { EnumerationType = type, Value = value, };
        }
    }
}
