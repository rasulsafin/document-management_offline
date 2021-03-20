using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldDtoToModelValueResolver : IValueResolver<DynamicFieldDto, IDynamicField, string>
    {
        public string Resolve(DynamicFieldDto source, IDynamicField destination, string destMember, ResolutionContext context)
        {
            if (source.Type == DynamicFieldType.ENUM)
                return (source.Value as Enumeration).Value.ID.ToString();

            if (source.Type == DynamicFieldType.OBJECT)
                return null;

            return source.Value.ToString();
        }
    }
}
