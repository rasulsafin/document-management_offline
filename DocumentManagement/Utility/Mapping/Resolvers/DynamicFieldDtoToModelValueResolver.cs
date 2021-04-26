using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class DynamicFieldDtoToModelValueResolver : IValueResolver<DynamicFieldDto, IDynamicField, string>
    {
        private readonly ILogger<DynamicFieldDtoToModelValueResolver> logger;

        public DynamicFieldDtoToModelValueResolver(ILogger<DynamicFieldDtoToModelValueResolver> logger)
        {
            this.logger = logger;
            logger.LogTrace("DynamicFieldDtoToModelValueResolver created");
        }

        public string Resolve(DynamicFieldDto source, IDynamicField destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.Type == DynamicFieldType.ENUM)
                return (source.Value as JObject).ToObject<Enumeration>().Value.ID.ToString();

            if (source.Type == DynamicFieldType.OBJECT)
                return null;

            return source.Value.ToString();
        }
    }
}
