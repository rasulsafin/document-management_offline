﻿using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldExternalDtoValueResolver : IValueResolver<DynamicFieldExternalDto, DynamicField, string>
    {
        private readonly DMContext dbContext;

        public DynamicFieldExternalDtoValueResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Resolve(DynamicFieldExternalDto source, DynamicField destination, string destMember, ResolutionContext context)
        {
            if (source.Type == DynamicFieldType.ENUM && source.Value != null)
                return dbContext.EnumerationValues.FirstOrDefault(x => x.ExternalId == source.Value).ID.ToString();

            return source.Value;
        }
    }
}