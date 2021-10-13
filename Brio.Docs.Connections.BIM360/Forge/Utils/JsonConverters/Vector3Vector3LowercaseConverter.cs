using Brio.Docs.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters
{
    public class Vector3Vector3LowercaseConverter : ANullableVector3Converter
    {
        public Vector3Vector3LowercaseConverter()
            : base(JsonToken.StartObject)
        {
        }

        protected override Vector3d ConvertTokenToVector3(JToken token)
            => new Vector3d(token.Value<double>("x"), token.Value<double>("y"), token.Value<double>("z"));

        protected override object ConvertVector3ToSerializingObject(Vector3d vector)
            => new
            {
                x = vector.X,
                y = vector.Y,
                z = vector.Z,
            };
    }
}
