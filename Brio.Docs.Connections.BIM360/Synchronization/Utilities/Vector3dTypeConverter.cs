using System;
using System.Globalization;
using Brio.Docs.Common;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    public class Vector3dTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
            => type == typeof(Vector3d);

        public object ReadYaml(IParser parser, Type type)
            => throw new NotImplementedException();

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var vector3d = (Vector3d)value;

            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(null, "x"));
            emitter.Emit(new Scalar(null, vector3d.X.ToString(CultureInfo.InvariantCulture)));
            emitter.Emit(new Scalar(null, "y"));
            emitter.Emit(new Scalar(null, vector3d.Y.ToString(CultureInfo.InvariantCulture)));
            emitter.Emit(new Scalar(null, "z"));
            emitter.Emit(new Scalar(null, vector3d.Z.ToString(CultureInfo.InvariantCulture)));
            emitter.Emit(new MappingEnd());
        }
    }
}
