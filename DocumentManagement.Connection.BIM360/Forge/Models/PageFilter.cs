using System.Text;

namespace Brio.Docs.Connection.Bim360.Forge.Models
{
    public class PageFilter : IQueryParameter
    {
        private readonly int limit;
        private readonly int offset;
        private readonly int number;

        private PageFilter(int limit = 100, int offset = -1, int number = -1)
        {
            this.limit = limit;
            this.offset = offset;
            this.number = number;
        }

        public static PageFilter ByNumber(int limit, int number)
            => new (limit, number: number);

        public static PageFilter ByOffset(int limit, int offset)
            => new (limit, offset);

        /// <inheritdoc cref="IQueryParameter.ToQueryString" />
        public string ToQueryString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(Constants.PAGE_QUERY_PARAMETER, Constants.LIMIT_PARAMETER_NAME, limit);
            if (offset >= 0)
                builder.AppendFormat(Constants.PAGE_QUERY_PARAMETER, Constants.OFFSET_PARAMETER_NAME, offset);
            if (number >= 0)
                builder.AppendFormat(Constants.PAGE_QUERY_PARAMETER, Constants.NUMBER_QUERY_PARAMETER, number);
            return builder.ToString();
        }
    }
}
