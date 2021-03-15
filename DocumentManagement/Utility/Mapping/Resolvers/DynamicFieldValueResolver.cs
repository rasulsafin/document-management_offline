﻿using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldValueResolver : IValueResolver<DynamicField, DynamicFieldExternalDto, string>
    {
        private readonly DMContext dbContext;

        public DynamicFieldValueResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Resolve(DynamicField source, DynamicFieldExternalDto destination, string destMember, ResolutionContext context)
        {
            if (source.Type == DynamicFieldType.ENUM.ToString())
                return dbContext.EnumerationValues.FirstOrDefault(x => x.ID == int.Parse(source.Value)).ExternalId;

            return source.Value;
        }
    }
}