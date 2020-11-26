using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Interface
{
    /// <summary>
    /// Entry point to DocumentManagement API
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
        Task<IAuthenticatedAccess> Register(UserToCreate data);
    }
}
