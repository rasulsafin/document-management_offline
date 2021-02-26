using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface
{
    /// <summary>
    /// Represents working with document management entities.
    /// </summary>
    /// <typeparam name="T">The type of entities.</typeparam>
    public interface ISynchronizer<T>
    {
        /// <summary>
        /// Add entity to external connection.
        /// </summary>
        /// <param name="obj">Entity to be added.</param>
        /// <returns>The added entity.</returns>
        Task<T> Add(T obj);

        /// <summary>
        /// Remove entity from external connection.
        /// </summary>
        /// <param name="obj">Entity to be removed.</param>
        /// <returns>The removed entity.</returns>
        Task<T> Remove(T obj);

        /// <summary>
        /// Update entity from external connection.
        /// </summary>
        /// <param name="obj">Entity to be updated.</param>
        /// <returns>The updated entity.</returns>
        Task<T> Update(T obj);
    }
}
