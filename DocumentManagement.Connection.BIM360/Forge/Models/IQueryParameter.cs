namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    /// <summary>
    /// Implements string query parameter, e.g., filter.
    /// </summary>
    public interface IQueryParameter
    {
        /// <summary>
        /// Gets the query string for a url.
        /// </summary>
        /// <returns>The query string for a request.</returns>
        string ToString();
    }
}
