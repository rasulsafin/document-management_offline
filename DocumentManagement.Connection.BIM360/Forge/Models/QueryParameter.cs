namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    public class QueryParameter : IQueryParameter
    {
        private readonly string key;
        private readonly string value;

        public QueryParameter(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        /// <inheritdoc cref="IQueryParameter.ToQueryString" />
        public string ToQueryString()
            => string.Format(
                Constants.QUERY_PARAMETER,
                key,
                value);
    }
}
