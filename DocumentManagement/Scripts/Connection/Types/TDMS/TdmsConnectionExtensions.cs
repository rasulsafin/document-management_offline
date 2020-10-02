using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MRS.Bim.DocumentManagement.Tdms
{
    public static class TdmsConnectionExtensions
    {
        public static string ToShortString(this MethodBase methodInfo)
        {
            var typeName = methodInfo.GetParameters().Select(x => x.ParameterType.ToShortString()).AggregateWithComma();
            return $"{methodInfo.Name}({typeName})";
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static string ToShortString(this Type type)
        {
            return !type.IsGenericType
                    ? type.Name
                    : $"{type.Name}[{type.GenericTypeArguments.Select(ToShortString).AggregateWithComma()}]";
        }

        private static string AggregateWithComma(this IEnumerable<string> types)
        {
            var array = types as string[] ?? types.ToArray();
            return array.Any() ? array.Aggregate((res, item) => $"{res}, {item}") : "";
        }
    }
}