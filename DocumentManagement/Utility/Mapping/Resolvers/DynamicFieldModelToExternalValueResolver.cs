using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldModelToExternalValueResolver : IValueResolver<IDynamicField, DynamicFieldExternalDto, string>
    {
        private readonly DMContext dbContext;

        public DynamicFieldModelToExternalValueResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Resolve(IDynamicField source, DynamicFieldExternalDto destination, string destMember, ResolutionContext context)
        {
            if (source.Type == DynamicFieldType.ENUM.ToString())
                return dbContext.EnumerationValues.FirstOrDefault(x => x.ID == int.Parse(source.Value)).ExternalId;

            return source.Value;
        }
    }
}