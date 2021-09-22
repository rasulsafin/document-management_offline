using System.Collections.Generic;
using System.Xml.Linq;

namespace Brio.Docs.Utils.ReportCreator.Attributes
{
    internal static class Style
    {
        private static Dictionary<string, IAttribute> attributes = new Dictionary<string, IAttribute>()
        {
            { "bold", new BoldAttribute()},
            { "center", new CenterAttribute()},
            { "right", new RightAttribute()},
        };

        public static IAttribute GetAttribute(XAttribute xAttribute) => attributes.TryGetValue(xAttribute.Value, out IAttribute value) ? value : null;
    }
}
