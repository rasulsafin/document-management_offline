using System;
using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    public class NullableVector3StringArrayConverter : ANullableVector3Converter
    {
        public NullableVector3StringArrayConverter()
            : base(JsonToken.StartArray)
        {
        }

        protected override Vector3 ConvertTokenToVector3(JToken token)
        {
            var items = token.ToObject<string[]>();

            if (items == null || items.Length != 3)
            {
                throw new NotSupportedException(
                    "Supports three-dimensional vectors represented only by the string array");
            }

            if (float.TryParse(items[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(items[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(items[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var z))
                return new Vector3(x, y, z);

            return Vector3.Zero;
        }

        protected override object ConvertVector3ToSerializingObject(Vector3 vector)
            => new[]
            {
                vector.X.ToString(CultureInfo.InvariantCulture),
                vector.Y.ToString(CultureInfo.InvariantCulture),
                vector.Z.ToString(CultureInfo.InvariantCulture),
            };
    }
}
