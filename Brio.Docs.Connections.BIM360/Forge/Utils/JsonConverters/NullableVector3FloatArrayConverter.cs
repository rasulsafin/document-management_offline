using System;
using Brio.Docs.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters
{
    public class NullableVector3FloatArrayConverter : ANullableVector3Converter
    {
        public NullableVector3FloatArrayConverter()
            : base(JsonToken.StartArray)
        {
        }

        protected override Vector3d ConvertTokenToVector3(JToken token)
        {
            var items = token.ToObject<double[]>();

            if (items == null || items.Length != 3)
            {
                throw new NotSupportedException(
                    "Supports three-dimensional vectors represented only by the float array");
            }

            return new Vector3d(items[0], items[1], items[2]);
        }

        protected override object ConvertVector3ToSerializingObject(Vector3d vector)
            => new[] { vector.X, vector.Y, vector.Z };
    }
}
