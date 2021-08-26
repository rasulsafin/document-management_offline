using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public static class DataMemberUtilities
    {
        public static string GetDataMemberName(PropertyInfo propertyInfo)
            => GetDataMemberNamePrivate(propertyInfo);

        public static string GetPath<T>(Expression<Func<T, object>> property)
        {
            var builder = new StringBuilder();

            var expression = property.Body;

            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                expression = unaryExpression.Operand;

            while (expression is not ParameterExpression)
            {
                if (expression is not MemberExpression { Member: PropertyInfo propertyInfo } prop)
                    throw new ArgumentException("The lambda expression must use properties only", nameof(property));

                if (builder.Length != 0)
                    builder.Insert(0, '.');
                builder.Insert(0, GetDataMemberName(propertyInfo));
                expression = prop.Expression;
            }

            return builder.ToString();
        }

        private static string GetDataMemberNamePrivate(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentException("Property doesn't exist", nameof(propertyInfo));

            var type = propertyInfo.DeclaringType;
            var name = propertyInfo.Name;

            while (true)
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), true);
                var dataMember = attributes.FirstOrDefault();

                // Recursiveness is required because the parameter inherit=true at GetCustomAttributes is ignored for properties.
                // Reference: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.memberinfo.getcustomattributes?view=net-5.0#System_Reflection_MemberInfo_GetCustomAttributes_System_Type_System_Boolean_
                if (dataMember == null)
                {
                    if ((type = type?.BaseType) == null || (propertyInfo = type.GetProperty(name)) == null)
                        return name;
                    continue;
                }

                return ((DataMemberAttribute)dataMember).Name;
            }
        }
    }
}
