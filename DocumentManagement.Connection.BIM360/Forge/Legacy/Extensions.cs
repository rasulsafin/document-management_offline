using System;
using System.Collections.Generic;

namespace Forge.Legacy
{
    public static class Extensions
    {
        public static List<dynamic> GetValues(this DynamicDictionary dynamic) => dynamic.Items().Select(x => (dynamic)x.Value).ToList();

        public static DateTime? GetNullable(this DateTime date)
        {
            if (date == DateTime.MinValue)
                return null;
            else
                return date;
        }
    }
}