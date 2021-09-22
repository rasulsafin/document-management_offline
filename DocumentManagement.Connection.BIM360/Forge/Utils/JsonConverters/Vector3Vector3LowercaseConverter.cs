using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connection.Bim360.Forge.Utils
{
    public class Vector3Vector3LowercaseConverter : ANullableVector3Converter
    {
        public Vector3Vector3LowercaseConverter()
            : base(JsonToken.StartObject)
        {
        }

        protected override Vector3 ConvertTokenToVector3(JToken token)
            => new Vector3(token.Value<float>("x"), token.Value<float>("y"), token.Value<float>("z"));

        protected override object ConvertVector3ToSerializingObject(Vector3 vector)
            => new
            {
                x = vector.X,
                y = vector.Y,
                z = vector.Z,
            };
    }
}
