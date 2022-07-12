using Brio.Docs.Connections.Bim360.Forge.Models;

namespace Brio.Docs.Connections.Bim360.Forge.Extensions
{
    public static class ConvertingExtensions
    {
        public static ObjectInfo ToInfo<T1, T2>(this Object<T1, T2> obj)
            => new ObjectInfo
            {
                ID = obj.ID,
                Type = obj.Type,
            };

        public static T ToObject<T, TAttributes, TRelationships>(this ObjectInfo info)
                where T : Object<TAttributes, TRelationships>, new()
            => new T
            {
                ID = info.ID,
                Type = info.Type,
            };

        public static DataContainer<ObjectInfo> ToDataContainer(this ObjectInfo info)
            => new DataContainer<ObjectInfo>(info);
    }
}