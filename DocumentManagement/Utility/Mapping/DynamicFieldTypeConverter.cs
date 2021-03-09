using System;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldTypeConverter : ITypeConverter<IDynamicFieldDto, DynamicField>
    {
        private readonly IMapper mapper;

        public DynamicFieldTypeConverter(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public DynamicField Convert(IDynamicFieldDto source, DynamicField destination, ResolutionContext context)
        {
            return source.Type switch
            {
                DynamicFieldType.OBJECT => mapper.Map<DynamicFieldDto, DynamicField>(source as DynamicFieldDto),
                DynamicFieldType.BOOL => mapper.Map<BoolFieldDto, DynamicField>(source as BoolFieldDto),
                DynamicFieldType.STRING => mapper.Map<StringFieldDto, DynamicField>(source as StringFieldDto),
                DynamicFieldType.INTEGER => mapper.Map<IntFieldDto, DynamicField>(source as IntFieldDto),
                DynamicFieldType.FLOAT => mapper.Map<FloatFieldDto, DynamicField>(source as FloatFieldDto),
                DynamicFieldType.DATE => mapper.Map<DateFieldDto, DynamicField>(source as DateFieldDto),
                DynamicFieldType.ENUM => mapper.Map<EnumerationFieldDto, DynamicField>(source as EnumerationFieldDto),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
