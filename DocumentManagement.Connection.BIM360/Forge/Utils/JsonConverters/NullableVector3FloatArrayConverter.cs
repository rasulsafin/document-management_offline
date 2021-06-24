using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class NullableVector3FloatArrayConverter : ANullableVector3Converter
    {
        public NullableVector3FloatArrayConverter()
            : base(JsonToken.StartArray)
        {
        }

        protected override Vector3 ConvertTokenToVector3(JToken token)
        {
            var items = token.ToObject<float[]>();

            if (items == null || items.Length != 3)
            {
                throw new NotSupportedException(
                    "Supports three-dimensional vectors represented only by the float array");
            }

            return new Vector3(items[0], items[1], items[2]);
        }

        protected override object ConvertVector3ToSerializingObject(Vector3 vector)
            => new[] { vector.X, vector.Y, vector.Z };
    }
}
