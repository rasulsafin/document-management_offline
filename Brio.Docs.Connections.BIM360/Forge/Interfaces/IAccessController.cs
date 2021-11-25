using System.Threading;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    /// <summary>
    /// Represents a class that can check if a user has access.
    /// </summary>
    public interface IAccessController
    {
        /// <summary>
        /// Checks the access of the current user. Updates access as needed.
        /// </summary>
        /// <param name="token">The token for cancel the operation.</param>
        /// <returns>The task of the operation.</returns>
        public Task CheckAccessAsync(CancellationToken token);
    }
}
