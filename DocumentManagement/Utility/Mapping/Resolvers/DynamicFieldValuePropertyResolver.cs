using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldValuePropertyResolver : IValueResolver<DynamicField, EnumerationFieldDto, EnumerationValueDto>
    {
        private readonly DMContext dbContext;
        private readonly IMapper mapper;

        public DynamicFieldValuePropertyResolver(DMContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public EnumerationValueDto Resolve(DynamicField source, EnumerationFieldDto destination, EnumerationValueDto destMember, ResolutionContext context)
        {
            var enumValue = dbContext.EnumerationValues
                .Include(x => x.EnumerationType)
                .FirstOrDefault(x => x.ID == int.Parse(source.Value));

            return mapper.Map<EnumerationValueDto>(enumValue);
        }
    }
}