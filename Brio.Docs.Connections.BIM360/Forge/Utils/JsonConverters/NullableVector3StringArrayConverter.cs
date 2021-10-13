using System;
using System.Globalization;
using Brio.Docs.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters
{
    public class NullableVector3StringArrayConverter : ANullableVector3Converter
    {
        public NullableVector3StringArrayConverter()
            : base(JsonToken.StartArray)
        {
        }

        protected override Vector3d ConvertTokenToVector3(JToken token)
        {
            var items = token.ToObject<string[]>();

            if (items == null || items.Length != 3)
            {
                throw new NotSupportedException(
                    "Supports three-dimensional vectors represented only by the string array");
            }

            if (double.TryParse(items[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                double.TryParse(items[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y) &&
                double.TryParse(items[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var z))
                return new Vector3d(x, y, z);

            return Vector3d.Zero;
        }

        protected override object ConvertVector3ToSerializingObject(Vector3d vector)
            => new[]
            {
                vector.X.ToString(CultureInfo.InvariantCulture),
                vector.Y.ToString(CultureInfo.InvariantCulture),
                vector.Z.ToString(CultureInfo.InvariantCulture),
            };
    }
}
