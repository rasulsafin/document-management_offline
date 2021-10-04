using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Extensions
{
    public static class AttributesExtensions
    {
        public static string GetEnumMemberValue(this Enum value)
        {
            var attribute = value.GetAttribute<EnumMemberAttribute>();
            return attribute != null ? attribute.Value ?? value.ToString() : value.ToString();
        }

        public static T GetAttribute<T>(this Enum value)
            where T : Attribute
            => value.GetType()
               .GetMember(value.ToString())
               .First()
               .GetCustomAttributes(typeof(T), true)
               .FirstOrDefault() as T;
    }
}
