using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions
{
    public static class DataExtensions
    {
        public static string GetDataMemberName(this Type type, string property)
        {
            while (true)
            {
                var propertyInfo = type.GetProperty(property);
                if (propertyInfo == null)
                    throw new ArgumentException("Property doesn't exist");

                var attributes = propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), true);
                var dataMember = attributes.FirstOrDefault();

                // Recursiveness is required because the parameter inherit=true at GetCustomAttributes is ignored for properties.
                // Reference: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.memberinfo.getcustomattributes?view=net-5.0#System_Reflection_MemberInfo_GetCustomAttributes_System_Type_System_Boolean_
                if (dataMember == null)
                {
                    if (type.BaseType == null)
                        return property;
                    type = type.BaseType;
                    continue;
                }

                return ((DataMemberAttribute)dataMember).Name;
            }
        }

        public static string GetPathOfDataMembers(this (Type type, string property)[] properties)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < properties.Length; i++)
            {
                builder.Append(properties[i].type.GetDataMemberName(properties[i].property));
                if (i != properties.Length - 1)
                    builder.Append('.');
            }

            return builder.ToString();
        }
    }
}
