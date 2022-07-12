using System;
using System.Linq;
using System.Linq.Expressions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;

namespace Brio.Docs.Connections.Bim360.Forge.Models
{
    public class FieldsFilter<T, TAttributes, TRelationships> : IQueryParameter
        where T : Object<TAttributes, TRelationships>
    {
        private readonly string key;
        private readonly string[] values;

        public FieldsFilter(Expression<Func<TAttributes, object>> property)
        {
            this.key = GetKey();
            values = new[] { DataMemberUtilities.GetPath(property) };
        }

        public FieldsFilter(params Expression<Func<TAttributes, object>>[] properties)
        {
            this.key = GetKey();
            this.values = properties.Select(DataMemberUtilities.GetPath).ToArray();
        }

        /// <inheritdoc cref="IQueryParameter.ToQueryString" />
        public string ToQueryString()
            => string.Format(
                Constants.FIELDS_FILTER_QUERY_PARAMETER,
                key,
                string.Join(',', values));

        private static string GetKey()
        {
            var instance = Activator.CreateInstance<T>();
            return instance.Type;
        }
    }
}
