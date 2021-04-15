﻿using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class DynamicFieldExternalToModelValueResolver : IValueResolver<DynamicFieldExternalDto, IDynamicField, string>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<DynamicFieldExternalToModelValueResolver> logger;

        public DynamicFieldExternalToModelValueResolver(DMContext dbContext, ILogger<DynamicFieldExternalToModelValueResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("DynamicFieldExternalToModelValueResolver created");
        }

        public string Resolve(DynamicFieldExternalDto source, IDynamicField destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.Type == DynamicFieldType.ENUM && source.Value != null)
                return dbContext.EnumerationValues.FirstOrDefault(x => x.ExternalId == source.Value).ID.ToString();

            return source.Value;
        }
    }
}