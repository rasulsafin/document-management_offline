using System;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldDtoTypeConverter : ITypeConverter<DynamicField, IDynamicFieldDto>
    {
        private readonly IMapper mapper;

        public DynamicFieldDtoTypeConverter(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public IDynamicFieldDto Convert(DynamicField source, IDynamicFieldDto destination, ResolutionContext context)
        {
            return ((DynamicFieldType)Enum.Parse(typeof(DynamicFieldType), source.Type)) switch
            {
                DynamicFieldType.OBJECT => mapper.Map<DynamicField, DynamicFieldDto>(source),
                DynamicFieldType.BOOL => mapper.Map<DynamicField, BoolFieldDto>(source),
                DynamicFieldType.STRING => mapper.Map<DynamicField, StringFieldDto>(source),
                DynamicFieldType.INTEGER => mapper.Map<DynamicField, IntFieldDto>(source),
                DynamicFieldType.FLOAT => mapper.Map<DynamicField, FloatFieldDto>(source),
                DynamicFieldType.DATE => mapper.Map<DynamicField, DateFieldDto>(source),
                DynamicFieldType.ENUM => mapper.Map<DynamicField, EnumerationFieldDto>(source),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
