using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    /// <summary>
    /// Service for queuing long tasks.
    /// </summary>
    public interface IRequestQueueService
    {
        /// <summary>
        /// Gets progress from 0 to 1 indicating of Task completion status.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <returns>Progress from 0 to 1.</returns>
        Task<double> GetProgress(string id);

        /// <summary>
        /// Gets result of the complete task and destroys it.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <returns>Result of the complete task.</returns>
        Task<RequestResult> GetResult(string id);

        /// <summary>
        /// Cancels task.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task Cancel(string id);
    }
}
