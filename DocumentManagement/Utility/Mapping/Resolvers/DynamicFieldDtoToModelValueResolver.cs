using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldDtoToModelValueResolver : IValueResolver<DynamicFieldDto, IDynamicField, string>
    {
        public string Resolve(DynamicFieldDto source, IDynamicField destination, string destMember, ResolutionContext context)
        {
            if (source.Type == DynamicFieldType.ENUM)
                return (source.Value as JObject).ToObject<Enumeration>().Value.ID.ToString();

            if (source.Type == DynamicFieldType.OBJECT)
                return null;

            return source.Value.ToString();
        }
    }
}
