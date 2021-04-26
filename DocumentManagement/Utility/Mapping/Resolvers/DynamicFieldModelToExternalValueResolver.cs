using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class DynamicFieldModelToExternalValueResolver : IValueResolver<IDynamicField, DynamicFieldExternalDto, string>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<DynamicFieldModelToExternalValueResolver> logger;

        public DynamicFieldModelToExternalValueResolver(DMContext dbContext, ILogger<DynamicFieldModelToExternalValueResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("DynamicFieldModelToExternalValueResolver created");
        }

        public string Resolve(IDynamicField source, DynamicFieldExternalDto destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.Type == DynamicFieldType.ENUM.ToString())
                return dbContext.EnumerationValues.FirstOrDefault(x => x.ID == int.Parse(source.Value)).ExternalId;

            return source.Value;
        }
    }
}