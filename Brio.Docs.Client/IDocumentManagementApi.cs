using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Client
{
    /// <summary>
    /// Entry point to Brio.Docs API
    /// </summary>
    public interface IDocumentManagementApi
    {
        /// <summary>
        /// Get authenticated access to API as registered user
        /// </summary>
        /// <returns>Null if can not login with these credentials</returns>
        Task<IAuthenticatedAccess> Login(string login, string password);
        /// <summary>
        /// Get authenticated access to API as new user
        /// </summary>
        /// <param name="data">New user credentials</param>
        /// <returns>Null if registration failed</returns>
        Task<IAuthenticatedAccess> Register(UserToCreateDto data);
    }
}
